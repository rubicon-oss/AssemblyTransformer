// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyMethodsVirtualizer.ILCodeGeneration;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace AssemblyMethodsVirtualizer.UnitTests.ICodeGenerationTest
{
  [TestFixture]
  public class ILCodeGeneratorTest
  {
    private ILCodeGenerator _codeGen;
    private AssemblyDefinition _assm;
    private string _unspeakablePrefix;

    [SetUp]
    public void SetUp ()
    {
      _unspeakablePrefix = "<>unspeakable_";
      _codeGen = new ILCodeGenerator (_unspeakablePrefix);
      _assm = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition();
    }

    [Test]
    public void GenerateMethodAndMoveBody_createsMethod ()
    {
      var method = _assm.MainModule.Types[1].Methods[0];  
      var result = _codeGen.GenerateMethodAndMoveBody (method);

      Assert.That (result, Is.Not.Null);
      Assert.That (result.Name, Is.EqualTo (_unspeakablePrefix + method.Name));
      Assert.That (result.ReturnType, Is.EqualTo (method.ReturnType));
    }

    [Test]
    public void GenerateMethodAndMoveBody_movesBody ()
    {
      var method = _assm.MainModule.Types[1].Methods[0];
      Instruction[] origBody = new Instruction[method.Body.Instructions.Count];
      method.Body.Instructions.CopyTo (origBody, 0);
      
      var result = _codeGen.GenerateMethodAndMoveBody (method);
      _assm.MainModule.Types[1].Methods.Add (result);

      for (int i = 0; i < result.Body.Instructions.Count; i++)
        Assert.That (result.Body.Instructions[i].OpCode == origBody[i].OpCode);
    }

    [Test]
    public void GenerateMethodAndMoveBody_insertsCall ()
    {
      var method = _assm.MainModule.Types[1].Methods[0];
      var origBody = new MethodBody (method);

      var result = _codeGen.GenerateMethodAndMoveBody (method);
      _assm.MainModule.Types[1].Methods.Add (result);

      Assert.That (method.Body, Is.Not.Null);
      Assert.That (method.Body.Instructions[0].OpCode == OpCodes.Ldarg_0);
      Assert.That (method.Body.Instructions[1].OpCode == OpCodes.Callvirt);

      Assert.That (((MethodReference) method.Body.Instructions[1].Operand).ReturnType.FullName
        + " " + ((MethodReference) method.Body.Instructions[1].Operand).DeclaringType.FullName
        + "::" + ((MethodReference) method.Body.Instructions[1].Operand).Name + "()"
        == result.FullName);

      Assert.That (method.Body.Instructions[2].OpCode == OpCodes.Ret);
    }

  }
}