// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyMethodsVirtualizer.UnitTests;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.FileSystem;
using ConstructorGenerator;
using ConstructorGenerator.CodeGenerator;
using ConstructorGenerator.MixinChecker;
using ConstructorGenerator.ReferenceGenerator;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using Rhino.Mocks;

namespace ConstructorGeneratorUnitTests.CodeGeneratorTest
{
  [TestFixture]
  public class CodeGeneratorTest
  {
    private IFileSystem _fileSystemMock;
    private IAssemblyTracker _tracker;
    private AssemblyDefinition _assemblyDefinition1;

    private ICodeGenerator _codeGenerator;
    private IReferenceGenerator _referenceGenerator;
    private IMixinChecker _checker;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem> ();
      _assemblyDefinition1 = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyWithCtorsAndMethodBody();
      _tracker = MockRepository.GenerateStrictMock<IAssemblyTracker> ();

      _checker = MockRepository.GenerateStrictMock<IMixinChecker>();
      _referenceGenerator = MockRepository.GenerateStrictMock<IReferenceGenerator>();
      _codeGenerator = new ILCodeGenerator (_referenceGenerator, _checker);
    }

    [Test]
    public void CodeGenerator_ReplacesNew ()
    {
      var method = _assemblyDefinition1.MainModule.Types[1].Methods[2];
      var ctor = _assemblyDefinition1.MainModule.Types[1].Methods[1];
      _checker.Stub (mock => mock.CanBeMixed (Arg<string>.Is.Anything)).Return (true);
      _checker.Stub (mock => mock.IsCached (Arg<string>.Is.Anything)).Return (true);
      _referenceGenerator.Expect (r => r.GetCallableParamListCreateMethod (_assemblyDefinition1 ,_assemblyDefinition1.MainModule.Types[1].Methods[1], _tracker)).IgnoreArguments().Return (
        MethodReferenceObjectMother.GetParamListMethodRef (_assemblyDefinition1));
      _referenceGenerator.Expect (r => r.GetCallableObjectFactoryCreateMethod (_assemblyDefinition1, _assemblyDefinition1.MainModule, _assemblyDefinition1.MainModule.Types[1], _tracker)).Return (
          MethodReferenceObjectMother.GetObjectFactoryMethodRef (_assemblyDefinition1));

      var result = _codeGenerator.ReplaceNewStatements (_assemblyDefinition1, method.DeclaringType, method, _tracker);

      Assert.That (result, Is.True);
      Assert.That (method.Body.Instructions[0] != (Instruction.Create (OpCodes.Newobj, ctor)));
      Assert.That (method.Body.Instructions[1] != (Instruction.Create (OpCodes.Newobj, ctor)));
      Assert.That (method.Body.Instructions[1].OpCode == OpCodes.Call);
      Assert.That (((MethodReference)method.Body.Instructions[1].Operand).Name == "ParamList");
    }

    [Test]
    public void CodeGenerator_DoesnotReplaceAnything ()
    {
      var ctor = _assemblyDefinition1.MainModule.Types[1].Methods[1];

      var result = _codeGenerator.ReplaceNewStatements (_assemblyDefinition1 ,ctor.DeclaringType, ctor, _tracker);

      Assert.That (result, Is.False);
    }

    [Test]
    public void CodeGenerator_GeneratesNewobjectMethod ()
    {
      var ctor = _assemblyDefinition1.MainModule.Types[1].Methods[1];
      _referenceGenerator.Expect (r => r.GetCallableParamListCreateMethod (_assemblyDefinition1, _assemblyDefinition1.MainModule.Types[1].Methods[1], _tracker)).IgnoreArguments ().Return (
        MethodReferenceObjectMother.GetParamListMethodRef (_assemblyDefinition1));
      _referenceGenerator.Expect (r => r.GetCallableObjectFactoryCreateMethod (_assemblyDefinition1, _assemblyDefinition1.MainModule, _assemblyDefinition1.MainModule.Types[1], _tracker)).Return (
          MethodReferenceObjectMother.GetObjectFactoryMethodRef (_assemblyDefinition1));

      _codeGenerator.CreateNewObjectMethod (_assemblyDefinition1, ctor, _tracker);

      var method = _assemblyDefinition1.MainModule.Types[1].Methods[3];
      Assert.That (method != null);
      Assert.That (method.Name == "NewObject");
      Assert.That (method.Parameters.Count == 1);
    }

  }
}