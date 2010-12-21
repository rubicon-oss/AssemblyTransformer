// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Reflection;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;
using AssemblyTransformer.AssemblySigning.AssemblyWriting;

namespace AssemblyTransformer.UnitTests.AssemblyWriter
{
  [TestFixture]
  public class ModuleWriterTest
  {

    private IFileSystem _fileSystemMock;
    private ModuleWriter _writer;
    private StrongNameKeyPair _signKey;
    private StrongNameKeyPair _signKey2;
    private AssemblyDefinition _assemblyDefinition1;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem> ();
      _signKey = new StrongNameKeyPair (AssemblyNameReferenceObjectMother.PublicKey1);
      _signKey2 = new StrongNameKeyPair (AssemblyNameReferenceObjectMother.PublicKey2);
      _writer = new ModuleWriter (
          _fileSystemMock,
          _signKey,
          new List<StrongNameKeyPair> { });

      _assemblyDefinition1 = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition();
    }

    [Test]
    public void ModuleWriter_WriteModuleWasCalled_WithKey ()
    {
      _fileSystemMock
          .Expect (mock => mock.Move(_assemblyDefinition1.MainModule.FullyQualifiedName, _assemblyDefinition1.MainModule.FullyQualifiedName+".bak"));
      _fileSystemMock
          .Expect (mock => mock.WriteModuleDefinition (
            Arg<ModuleDefinition>.Is.Same (_assemblyDefinition1.MainModule),
            Arg<string>.Is.Same (_assemblyDefinition1.MainModule.FullyQualifiedName),
            Arg<WriterParameters>.Matches(param => param.StrongNameKeyPair.Equals(_signKey))));
      _fileSystemMock.Replay();

      _writer.WriteModule (_assemblyDefinition1.MainModule, _assemblyDefinition1.MainModule);

      _fileSystemMock.VerifyAllExpectations();
    }

    [Test]
    public void ModuleWriter_WriteModuleWasCalled_WithoutKey ()
    {
      _writer = new ModuleWriter (
          _fileSystemMock,
          null,
          new List<StrongNameKeyPair> { });
      _fileSystemMock
          .Expect (mock => mock.Move (_assemblyDefinition1.MainModule.FullyQualifiedName, _assemblyDefinition1.MainModule.FullyQualifiedName + ".bak"));
      _fileSystemMock
          .Expect (mock => mock.WriteModuleDefinition (
            Arg<ModuleDefinition>.Is.Same (_assemblyDefinition1.MainModule),
            Arg<string>.Is.Same (_assemblyDefinition1.MainModule.FullyQualifiedName),
            Arg<WriterParameters>.Matches (param => param.StrongNameKeyPair == null)));
      _fileSystemMock.Replay ();

      _writer.WriteModule (_assemblyDefinition1.MainModule, _assemblyDefinition1.MainModule);

      _fileSystemMock.VerifyAllExpectations ();
    }

  }
}