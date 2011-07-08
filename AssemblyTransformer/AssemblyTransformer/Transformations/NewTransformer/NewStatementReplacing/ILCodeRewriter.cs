// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq;
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using NewTransformer.InfoProvider;

namespace NewTransformer.NewStatementReplacing
{
  public class ILCodeRewriter : ICodeRewriter
  {
    
    
    public bool ReplaceNewStatements (AssemblyDefinition containingAssembly,
                                      TypeDefinition containingType,
                                      MethodDefinition targetMethod,
                                      IAssemblyTracker tracker,
                                      INewTransformerInfoWrapper infoWrapper)
    {
      var isModified = false;
      if (targetMethod.HasBody)
      {
        var instructions = targetMethod.Body.Instructions;

        for (int i = 0; i < instructions.Count; i++)
        {
          if (instructions[i].OpCode == OpCodes.Newobj)
          {
            var constructor = (MethodReference) instructions[i].Operand;
            MethodDefinition factoryMethod = null;

            // Expected factory method signature: public static T Create<T> (ParamList)
            if (!constructor.Parameters.Any (p => p.ParameterType == containingType.Module.TypeSystem.IntPtr) &&
                ((factoryMethod = infoWrapper.GetFactoryMethod (constructor, containingAssembly, tracker)) != null))
            {
              isModified = true;

              if (factoryMethod.GenericParameters.Count != 1 || factoryMethod.Parameters.Count != 1 || !factoryMethod.IsStatic)
                throw new ArgumentException ("Factory method to create object does not have correct signature [public static T Create<T> (ParamList)]");

              var importedFactoryMethod = containingType.Module.Import (factoryMethod);
              var genericInstanceMethod = new GenericInstanceMethod (importedFactoryMethod);
              genericInstanceMethod.GenericArguments.Add (constructor.DeclaringType);

              var paramlistDef = factoryMethod.Parameters[0].ParameterType.Resolve (); 
              var importedParamListCreateMethod = containingType.Module.Import (SearchParamListFactoryMethod (paramlistDef, constructor));

              if (importedParamListCreateMethod == null)
                throw new ArgumentException ("Factory method: no corresponding 'create' method could have been found. [argument count]");

              #region ILsample

              // 1: ldstr "Test"
              // 2: newObj
              // 3: next
              // ::---- becomes ->>
              // 1: ldstr "Test"
              // 2: nop (original call statement - kept as branch target)
              // 3: call ParamList.Create<string> (string)
              // 4: call ObjectFactory
              // 5: next

              #endregion

              targetMethod.Body.SimplifyMacros ();

              instructions[i].OpCode = OpCodes.Nop;
              instructions.Insert (i + 1, Instruction.Create (OpCodes.Call, importedParamListCreateMethod));
              instructions.Insert (i + 2, Instruction.Create (OpCodes.Call, genericInstanceMethod));

              targetMethod.Body.OptimizeMacros ();
            }
          }
        }
      }
      return isModified;
    }

    public void CreateNewObjectMethod (AssemblyDefinition assembly, 
                                        MethodDefinition templateMethod, 
                                        IAssemblyTracker tracker, 
                                        INewTransformerInfoWrapper infoWrapper)
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

      MethodDefinition factoryMethod = null;
      if ((factoryMethod = infoWrapper.GetFactoryMethod (templateMethod, assembly, tracker)) != null)
      {
        if (factoryMethod.GenericParameters.Count != 1 || factoryMethod.Parameters.Count != 1 || !factoryMethod.IsStatic)
          throw new ArgumentException ("Factory method to create object does not have correct signature [public static T Create<T> (ParamList)]");

        var importedFactoryMethod = templateMethod.Module.Import (factoryMethod);
        var genericInstanceMethod = new GenericInstanceMethod (importedFactoryMethod);
        genericInstanceMethod.GenericArguments.Add (templateMethod.DeclaringType);

        var paramlistDef = factoryMethod.Parameters[0].ParameterType.Resolve ();
        var importedParamListCreateMethod = templateMethod.Module.Import (SearchParamListFactoryMethod (paramlistDef, templateMethod));

        if (importedParamListCreateMethod == null)
          throw new ArgumentException ("Factory method: no corresponding 'create' method could have been found. [argument count]");


        var newObjectMethod = new MethodDefinition (
                                                    infoWrapper.GetWrapperMethodName (templateMethod), 
                                                    MethodAttributes.Public | MethodAttributes.Static, returnType
                                                    );
        var instructions = newObjectMethod.Body.Instructions;

        foreach (var param in templateMethod.Parameters)
        {
          newObjectMethod.Parameters.Add (param);
          instructions.Add (Instruction.Create (OpCodes.Ldarg, param));
        }

        instructions.Add(Instruction.Create (OpCodes.Call, importedParamListCreateMethod));
        instructions.Add(Instruction.Create (OpCodes.Call, genericInstanceMethod));

        instructions.Add (Instruction.Create (OpCodes.Ret));
        newObjectMethod.Body.OptimizeMacros ();

        newObjectMethod.IsHideBySig = true;
        templateMethod.DeclaringType.Methods.Add (newObjectMethod);
      }
    }


    private MethodReference SearchParamListFactoryMethod (TypeDefinition paramListTypeDefinition, MethodReference ctor)
    {
      MethodDefinition createMethodDefinition =
        paramListTypeDefinition.Methods.FirstOrDefault (m => m.Name == "Create" &&
                                      m.Parameters.Count == ctor.Parameters.Count &&
                                      m.GenericParameters.Count == ctor.Parameters.Count);
      if (createMethodDefinition != null)
      {
        if (createMethodDefinition.HasGenericParameters)
        {
          var genericInstanceMethod = new GenericInstanceMethod (createMethodDefinition);
          foreach (var param in ctor.Parameters)
            genericInstanceMethod.GenericArguments.Add (param.ParameterType);
          return genericInstanceMethod;
        }
      }
      return createMethodDefinition;
    }

  }
}