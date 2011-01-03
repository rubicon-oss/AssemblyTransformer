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

    // TODO Review FS: Name the tests like this: MethodName_AdditionalInfo_AndOtherInfo_AndThirdInfo; eg., WriteModule_UnsignedAssembly_NoKeyUsed, WriteModule_AssemblyWithUnknownKey_UsesDefaultKey()

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

    // TODO Review FS: I'd add tests for the following situations:
    // TODO Review FS: - main module of assembly with public key is passed in and a match is found in the list of key pairs
    // TODO Review FS: - main module of assembly with public key is passed in and no match is found in the list of key pairs - default key is used
    // TODO Review FS: - main module of assembly with public key is passed in, no match is found in the list of key pairs, but there is no default key - is this even allowed? There should probably be an exception in this case?
    // TODO Review FS: - secondary module of signed assembly is passed in
    // TODO Review FS: - secondary module of unsigned assembly is passed in
  }
}