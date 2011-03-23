// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ConstructorGeneratorUnitTests
{
  public static class MethodReferenceObjectMother
  {
    public static MethodReference GetParamListMethodRef (AssemblyDefinition assembly)
    {
      MethodDefinition method = new MethodDefinition ("ParamList", MethodAttributes.Public | MethodAttributes.HideBySig, assembly.MainModule.TypeSystem.Void);
      var body = method.Body.Instructions;
      body.Add (Instruction.Create (OpCodes.Ret));
      return method;
    }

    public static MethodReference GetObjectFactoryMethodRef (AssemblyDefinition assembly)
    {
      MethodDefinition method = new MethodDefinition ("ObjectFactory", MethodAttributes.Public | MethodAttributes.HideBySig, assembly.MainModule.TypeSystem.Void);
      var body = method.Body.Instructions;
      body.Add (Instruction.Create (OpCodes.Ret));
      return method;
    }
  }
}