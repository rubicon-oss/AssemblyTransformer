// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.Extensions;
using ConstructorGenerator.MixinChecker;
using ConstructorGenerator.ReferenceGenerator;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace ConstructorGenerator.CodeGenerator
{
  public class ILCodeGenerator : ICodeGenerator
  {
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly IMixinChecker _checker;

    public IReferenceGenerator ReferenceGenerator
    {
      get { return _referenceGenerator; }
    }

    public ILCodeGenerator (IReferenceGenerator referenceGenerator, IMixinChecker checker)
    {
      _referenceGenerator = referenceGenerator;
      _checker = checker;
    }

    public bool ReplaceNewStatements (AssemblyDefinition containingAssembly, TypeDefinition containingType, MethodDefinition targetMethod, IAssemblyTracker tracker)
    {
      var isModified = false;
      if (targetMethod.HasBody)
      {
        var instructions = targetMethod.Body.Instructions;
        VariableDefinition paramListLocal = null;

        for (int i = 0; i < instructions.Count; i++)
        {
          if (instructions[i].OpCode == OpCodes.Newobj)
          {
            var constructor = (MethodReference) instructions[i].Operand;
            var assemblyQualifiedName = containingAssembly.Name.BuildReflectionAssemblyQualifiedName (constructor.DeclaringType);

            if (!constructor.Parameters.Any (p => p.ParameterType == containingType.Module.TypeSystem.IntPtr) &&
                _checker.IsCached (assemblyQualifiedName) &&
                _checker.CanBeMixed (assemblyQualifiedName))
            {
              targetMethod.Body.SimplifyMacros ();
              isModified = true;

              var objectFactory = _referenceGenerator.GetCallableObjectFactoryCreateMethod (containingAssembly, containingType.Module, constructor.DeclaringType, tracker);
              var paramListCreateReference = _referenceGenerator.GetCallableParamListCreateMethod (containingAssembly, constructor, tracker);

              if (paramListLocal == null)
              {
                targetMethod.Body.InitLocals = true;
                paramListLocal = new VariableDefinition ("__paramList", paramListCreateReference.ReturnType);
                targetMethod.Body.Variables.Add (paramListLocal);
              }

              #region ILsample

              // 1: ldstr "Test"
              // 2: newObj
              // 3: next
              // ::---- becomes ->>
              // 1: ldstr "Test"
              // 2: nop (original call statement - kept as branch target)
              // 3: call ParamList.Create<string> (string)
              // 4: stloc tempPList
              // 5: ldtrue
              // 6: ldloc tempPList
              // 7: ldc.i4.0
              // 8: newarr object
              // 9: call ObjectFactory
              // 10: next

              #endregion

              instructions[i].OpCode = OpCodes.Nop;

              var objectFactoryInstructions = GetObjectFactoryCreateInstructions (
                  objectFactory, containingType, paramListCreateReference, paramListLocal);
              for (int j = 0; j < objectFactoryInstructions.Length; ++j)
                instructions.Insert (i + 1 + j, objectFactoryInstructions[j]);
              
              targetMethod.Body.OptimizeMacros ();
            }
          }
        }
      }
      return isModified;
    }

    public void CreateNewObjectMethod (AssemblyDefinition containingAssembly, MethodDefinition templateMethod, IAssemblyTracker tracker)
    {
      TypeReference returnType = templateMethod.DeclaringType;
      if (templateMethod.DeclaringType.HasGenericParameters)
      {
        returnType = new GenericInstanceType (templateMethod.DeclaringType);
        foreach (var a in templateMethod.DeclaringType.GenericParameters.ToArray ())
        {
          returnType.GenericParameters.Add (a);
          ((GenericInstanceType) returnType).GenericArguments.Add (a);
        }
      }

      var objectFactory = _referenceGenerator.GetCallableObjectFactoryCreateMethod (containingAssembly, templateMethod.DeclaringType.Module, returnType, tracker);
      var paramListCreateReference = _referenceGenerator.GetCallableParamListCreateMethod (containingAssembly, templateMethod, tracker);

      var newObjectMethod = new MethodDefinition ("NewObject", MethodAttributes.Public | MethodAttributes.Static, returnType);
      var instructions = newObjectMethod.Body.Instructions;

      newObjectMethod.Body.InitLocals = true;
      var paramListLocal = new VariableDefinition ("__paramList", paramListCreateReference.ReturnType);
      newObjectMethod.Body.Variables.Add (paramListLocal);

      foreach (var param in templateMethod.Parameters)
      {
        newObjectMethod.Parameters.Add (param);
        instructions.Add (Instruction.Create (OpCodes.Ldarg, param));
      }

      var createInstructions = GetObjectFactoryCreateInstructions (objectFactory, templateMethod.DeclaringType, paramListCreateReference, paramListLocal);
      foreach (var instruction in createInstructions)
        instructions.Add (instruction);

      instructions.Add (Instruction.Create (OpCodes.Ret));
      newObjectMethod.Body.OptimizeMacros ();

      templateMethod.IsPrivate = false;
      templateMethod.IsPublic = false;
      templateMethod.IsFamily = true;
      newObjectMethod.IsHideBySig = true;
      templateMethod.DeclaringType.Methods.Add (newObjectMethod);
    }

    private Instruction[] GetObjectFactoryCreateInstructions (
        MethodReference objectFactory,
        TypeDefinition mixinTargetType,
        MethodReference paramListCreateReference,
        VariableDefinition paramListLocal)
    {
      return 
        new[] {
                Instruction.Create (OpCodes.Call, paramListCreateReference),
                Instruction.Create (OpCodes.Stloc, paramListLocal),
                Instruction.Create (OpCodes.Ldc_I4_1),
                Instruction.Create (OpCodes.Ldloc, paramListLocal),
                Instruction.Create (OpCodes.Ldc_I4_0),
                Instruction.Create (OpCodes.Newarr, mixinTargetType.Module.TypeSystem.Object),
                Instruction.Create (OpCodes.Call, objectFactory)
              };
    }

  }
}