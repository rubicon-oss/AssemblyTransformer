// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyMethodsVirtualizer.UnitTests;
using AssemblyTransformer;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.FileSystem;
using ConstructorGenerator;
using ConstructorGenerator.CodeGenerator;
using ConstructorGenerator.MixinChecker;
using ConstructorGenerator.ReferenceGenerator;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;

namespace ConstructorGeneratorUnitTests.ConstructorGeneratorTest
{
  [TestFixture]
  public class ConstructorGeneratorTest
  {
    private IFileSystem _fileSystemMock;
    private IAssemblyTracker _tracker;

    private AssemblyDefinition _assemblyDefinition1;
    private ConstructorGenerator.ConstructorGenerator _generator;
    private IMixinChecker _checker;
    private ICodeGenerator _codeGenerator;
    private IReferenceGenerator _referenceGenerator;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem> ();
      _assemblyDefinition1 = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition ();
      _tracker = MockRepository.GenerateStrictMock<IAssemblyTracker>();

      _checker = MockRepository.GenerateMock<IMixinChecker>();

      //_referenceGenerator = MockRepository.GenerateStrictMock<IReferenceGenerator>();
      _codeGenerator = MockRepository.GenerateStrictMock<ICodeGenerator>();

      _generator = new ConstructorGenerator.ConstructorGenerator (_checker, _codeGenerator);
    }

    [Test]
    public void Transforms_All_NoCtorsAvailable ()
    {
      _checker.Stub (mock => mock.CanBeMixed (Arg<string>.Is.Anything)).Return (true);
      _tracker.Expect (tracker => tracker.GetAssemblies ()).Return (new[] { _assemblyDefinition1 });
      _codeGenerator.Expect (generator => generator.CreateNewObjectMethod
        (_assemblyDefinition1, _assemblyDefinition1.MainModule.Types[1].Methods[0], _tracker));
      _codeGenerator.Expect (generator => generator.CreateNewObjectMethod 
        (_assemblyDefinition1, _assemblyDefinition1.Modules[1].Types[1].Methods[0], _tracker));
      _tracker.Expect (tracker => tracker.GetAssemblies ()).Return (new[] { _assemblyDefinition1 });
      _codeGenerator.Expect (generator => generator.ReplaceNewStatements
        (_assemblyDefinition1, _assemblyDefinition1.MainModule.Types[1], _assemblyDefinition1.MainModule.Types[1].Methods[0], _tracker)).Return (true);
      _tracker.Expect (tracker => tracker.MarkModified (_assemblyDefinition1));
      _codeGenerator.Expect (generator => generator.ReplaceNewStatements
        (_assemblyDefinition1, _assemblyDefinition1.Modules[1].Types[1], _assemblyDefinition1.Modules[1].Types[1].Methods[0], _tracker)).Return (true);
      _tracker.Expect (tracker => tracker.MarkModified (_assemblyDefinition1));

      _generator.Transform (_tracker);

      _tracker.VerifyAllExpectations();
    }

    [Test]
    public void Transforms_NoNewObjects_ReplacesAllNewStatements ()
    {
      _checker.Stub (mock => mock.CanBeMixed (Arg<string>.Is.Anything)).Return (false);
      _tracker.Expect (tracker => tracker.GetAssemblies ()).Return (new[] { _assemblyDefinition1 });
      _tracker.Expect (tracker => tracker.GetAssemblies ()).Return (new[] { _assemblyDefinition1 });
      _codeGenerator.Expect (generator => generator.ReplaceNewStatements
        (_assemblyDefinition1, _assemblyDefinition1.MainModule.Types[1], _assemblyDefinition1.MainModule.Types[1].Methods[0], _tracker)).Return (true);
      _tracker.Expect (tracker => tracker.MarkModified (_assemblyDefinition1));
      _codeGenerator.Expect (generator => generator.ReplaceNewStatements
        (_assemblyDefinition1, _assemblyDefinition1.Modules[1].Types[1], _assemblyDefinition1.Modules[1].Types[1].Methods[0], _tracker)).Return (true);
      _tracker.Expect (tracker => tracker.MarkModified (_assemblyDefinition1));

      _generator.Transform (_tracker);

      _tracker.VerifyAllExpectations ();
    }

    [Test]
    public void Transforms_None ()
    {
      _checker.Stub (mock => mock.CanBeMixed (Arg<string>.Is.Anything)).Return (false);
      _tracker.Expect (tracker => tracker.GetAssemblies ()).Return (new[] { _assemblyDefinition1 });
      _tracker.Expect (tracker => tracker.GetAssemblies ()).Return (new[] { _assemblyDefinition1 });
      _codeGenerator.Expect (generator => generator.ReplaceNewStatements
        (_assemblyDefinition1, _assemblyDefinition1.MainModule.Types[1], _assemblyDefinition1.MainModule.Types[1].Methods[0], _tracker)).Return (false);
      _codeGenerator.Expect (generator => generator.ReplaceNewStatements
        (_assemblyDefinition1, _assemblyDefinition1.Modules[1].Types[1], _assemblyDefinition1.Modules[1].Types[1].Methods[0], _tracker)).Return (false);

      _generator.Transform (_tracker);

      _tracker.VerifyAllExpectations ();
    }

  }
}