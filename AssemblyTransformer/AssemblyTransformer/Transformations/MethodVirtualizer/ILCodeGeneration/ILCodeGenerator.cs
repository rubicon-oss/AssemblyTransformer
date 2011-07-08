// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace MethodVirtualizer.ILCodeGeneration
{
  public class ILCodeGenerator : ICodeGenerator
  {
    public ILCodeGenerator ()
    {
    }

    public MethodDefinition GenerateMethodAndMoveBody (MethodDefinition originalMethod, string introducedMethodName)
    {
      TypeReference returnType = originalMethod.ReturnType;
      if (originalMethod.ReturnType.HasGenericParameters)
      {
        returnType = new GenericInstanceType (originalMethod.ReturnType);
        foreach (var a in originalMethod.ReturnType.GenericParameters.ToArray ())
        {
          returnType.GenericParameters.Add (a);
          ((GenericInstanceType) returnType).GenericArguments.Add (a);
        }
      }
      var virtualMethodName = introducedMethodName;
      int counter = 0;
      while (originalMethod.DeclaringType.Methods.Any (m => m.Name == virtualMethodName))
        virtualMethodName = counter++ + introducedMethodName;

      var virtualMethod = new MethodDefinition (
          virtualMethodName,
          MethodAttributes.Public | MethodAttributes.Virtual,
          returnType
          );

      virtualMethod.Body.InitLocals = true;

      originalMethod.Body.SimplifyMacros ();

      foreach (var v in originalMethod.Body.Variables)
        virtualMethod.Body.Variables.Add (v);
      foreach (var i in originalMethod.Body.Instructions)
        virtualMethod.Body.Instructions.Add (i);

      var origBodyInstructions = originalMethod.Body.Instructions;
      originalMethod.Body.Variables.Clear ();
      origBodyInstructions.Clear ();

      origBodyInstructions.Add (Instruction.Create (OpCodes.Ldarg_0));
      foreach (var param in originalMethod.Parameters)
      {
        virtualMethod.Parameters.Add (param);
        origBodyInstructions.Add (Instruction.Create (OpCodes.Ldarg, param));
      }
      origBodyInstructions.Add (Instruction.Create (OpCodes.Callvirt, virtualMethod));
      origBodyInstructions.Add (Instruction.Create (OpCodes.Ret));

      return virtualMethod;
    }
  }
}