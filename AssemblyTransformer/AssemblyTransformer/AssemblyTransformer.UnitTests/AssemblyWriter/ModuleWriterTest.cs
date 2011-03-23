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
    private ModuleDefinitionWriter _definitionWriter;
    private StrongNameKeyPair _signKey;
    private StrongNameKeyPair _signKey2;
    private AssemblyDefinition _assemblyDefinition1;
    private AssemblyDefinition _signedAssemblyDefinition;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem> ();
      _signKey = new StrongNameKeyPair (AssemblyNameReferenceObjectMother.PublicKey1);
      _signKey2 = new StrongNameKeyPair (AssemblyNameReferenceObjectMother.PublicKey2);
      _definitionWriter = new ModuleDefinitionWriter (
          _fileSystemMock,
          _signKey,
          new List<StrongNameKeyPair> { });

      _assemblyDefinition1 = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition ();
      _signedAssemblyDefinition = AssemblyDefinitionObjectMother.CreateSignedMultiModuleAssemblyDefinition ();
    }

    [Test]
    public void WriteModuleDefinition_WriterHasDefaultKey ()
    {
      _fileSystemMock
           .Expect (mock => mock.FileExists (_assemblyDefinition1.MainModule.FullyQualifiedName)).Return (true);
      _fileSystemMock
           .Expect (mock => mock.FileExists (_assemblyDefinition1.MainModule.FullyQualifiedName + ".bak0")).Return (false);
      _fileSystemMock
          .Expect (mock => mock.Move(_assemblyDefinition1.MainModule.FullyQualifiedName, _assemblyDefinition1.MainModule.FullyQualifiedName+".bak0"));
      _fileSystemMock
          .Expect (mock => mock.WriteModuleDefinition (
            Arg<ModuleDefinition>.Is.Same (_assemblyDefinition1.MainModule),
            Arg<string>.Is.Same (_assemblyDefinition1.MainModule.FullyQualifiedName),
            Arg<WriterParameters>.Matches(param => param.StrongNameKeyPair == null)));
      _fileSystemMock.Replay();

      _definitionWriter.WriteModule (_assemblyDefinition1.MainModule);

      _fileSystemMock.VerifyAllExpectations();
    }

    [Test]
    public void WriteModuleDefinition_NoDefaultKey_NoAvailableKeys ()
    {
      _definitionWriter = new ModuleDefinitionWriter (
          _fileSystemMock,
          null,
          new List<StrongNameKeyPair> { });
      _fileSystemMock
           .Expect (mock => mock.FileExists (_assemblyDefinition1.MainModule.FullyQualifiedName)).Return (true);
      _fileSystemMock
           .Expect (mock => mock.FileExists (_assemblyDefinition1.MainModule.FullyQualifiedName + ".bak0")).Return (false);
      _fileSystemMock
          .Expect (mock => mock.Move (_assemblyDefinition1.MainModule.FullyQualifiedName, _assemblyDefinition1.MainModule.FullyQualifiedName + ".bak0"));
      _fileSystemMock
          .Expect (mock => mock.WriteModuleDefinition (
            Arg<ModuleDefinition>.Is.Same (_assemblyDefinition1.MainModule),
            Arg<string>.Is.Same (_assemblyDefinition1.MainModule.FullyQualifiedName),
            Arg<WriterParameters>.Matches (param => param.StrongNameKeyPair == null)));
      _fileSystemMock.Replay ();

      _definitionWriter.WriteModule (_assemblyDefinition1.MainModule);

      _fileSystemMock.VerifyAllExpectations ();
    }

    [Test]
    public void WriteModuleDefinition_SignedAssembly_WriterFindsMatchingKey ()
    {
      _definitionWriter = new ModuleDefinitionWriter (
          _fileSystemMock,
          null,
          new List<StrongNameKeyPair> { AssemblyNameReferenceObjectMother.RealKeyPair() });
      _fileSystemMock
           .Expect (mock => mock.FileExists (_signedAssemblyDefinition.MainModule.FullyQualifiedName)).Return (true);
      _fileSystemMock
           .Expect (mock => mock.FileExists (_signedAssemblyDefinition.MainModule.FullyQualifiedName + ".bak0")).Return (false);
      _fileSystemMock
          .Expect (mock => mock.Move (_signedAssemblyDefinition.MainModule.FullyQualifiedName, _signedAssemblyDefinition.MainModule.FullyQualifiedName + ".bak0"));
      _fileSystemMock
          .Expect (mock => mock.WriteModuleDefinition (
            Arg<ModuleDefinition>.Is.Same (_signedAssemblyDefinition.MainModule),
            Arg<string>.Is.Same (_signedAssemblyDefinition.MainModule.FullyQualifiedName),
            Arg<WriterParameters>.Matches (param => param.StrongNameKeyPair.Equals (AssemblyNameReferenceObjectMother.RealKeyPair()))));
      _fileSystemMock.Replay ();

      _definitionWriter.WriteModule (_signedAssemblyDefinition.MainModule);

      _fileSystemMock.VerifyAllExpectations ();
    }

    [Test]
    public void WriteModuleDefinition_SignedAssembly_WriterHasDefaultKey ()
    {
      _fileSystemMock
           .Expect (mock => mock.FileExists (_signedAssemblyDefinition.MainModule.FullyQualifiedName)).Return (true);
      _fileSystemMock
           .Expect (mock => mock.FileExists (_signedAssemblyDefinition.MainModule.FullyQualifiedName + ".bak0")).Return (false);
      _fileSystemMock
          .Expect (mock => mock.Move (_signedAssemblyDefinition.MainModule.FullyQualifiedName, _assemblyDefinition1.MainModule.FullyQualifiedName + ".bak0"));
      _fileSystemMock
          .Expect (mock => mock.WriteModuleDefinition (
            Arg<ModuleDefinition>.Is.Same (_signedAssemblyDefinition.MainModule),
            Arg<string>.Is.Same (_signedAssemblyDefinition.MainModule.FullyQualifiedName),
            Arg<WriterParameters>.Matches (param => param.StrongNameKeyPair.Equals (_signKey))));
      _fileSystemMock.Replay ();

      _definitionWriter.WriteModule (_signedAssemblyDefinition.MainModule);

      _fileSystemMock.VerifyAllExpectations ();
    }

    [Test]
    public void WriteModuleDefinition_SignedAssembly_WriterHasNoKeys ()
    {
      _definitionWriter = new ModuleDefinitionWriter (
          _fileSystemMock,
          null,
          new List<StrongNameKeyPair> { });
      _fileSystemMock
           .Expect (mock => mock.FileExists (_signedAssemblyDefinition.MainModule.FullyQualifiedName)).Return (true);
      _fileSystemMock
           .Expect (mock => mock.FileExists (_signedAssemblyDefinition.MainModule.FullyQualifiedName + ".bak0")).Return (false);
      _fileSystemMock
          .Expect (mock => mock.Move (_signedAssemblyDefinition.MainModule.FullyQualifiedName, _assemblyDefinition1.MainModule.FullyQualifiedName + ".bak0"));
      _fileSystemMock
          .Expect (mock => mock.WriteModuleDefinition (
            Arg<ModuleDefinition>.Is.Same (_signedAssemblyDefinition.MainModule),
            Arg<string>.Is.Same (_signedAssemblyDefinition.MainModule.FullyQualifiedName),
            Arg<WriterParameters>.Matches (param => param.StrongNameKeyPair == null)));
      _fileSystemMock.Replay ();

      _definitionWriter.WriteModule (_signedAssemblyDefinition.MainModule);

      _fileSystemMock.VerifyAllExpectations ();
    }

    [Test]
    public void WriteModuleDefinition_SignedAssembly_SecondaryModule_WriterHasDefaultKey ()
    {
      _fileSystemMock
           .Expect (mock => mock.FileExists (_signedAssemblyDefinition.Modules[1].FullyQualifiedName)).Return (true);
      _fileSystemMock
           .Expect (mock => mock.FileExists (_signedAssemblyDefinition.Modules[1].FullyQualifiedName + ".bak0")).Return (false);
      _fileSystemMock
          .Expect (mock => mock.Move (_signedAssemblyDefinition.Modules[1].FullyQualifiedName, _assemblyDefinition1.Modules[1].FullyQualifiedName + ".bak0"));
      _fileSystemMock
          .Expect (mock => mock.WriteModuleDefinition (
            Arg<ModuleDefinition>.Is.Same (_signedAssemblyDefinition.Modules[1]),
            Arg<string>.Is.Same (_signedAssemblyDefinition.Modules[1].FullyQualifiedName),
            Arg<WriterParameters>.Matches (param => param.StrongNameKeyPair == null)));
      _fileSystemMock.Replay ();

      _definitionWriter.WriteModule (_signedAssemblyDefinition.Modules[1]);

      _fileSystemMock.VerifyAllExpectations ();
    }

    [Test]
    public void WriteModuleDefinition_SecondaryModule_WriterHasDefaultKey ()
    {
      _fileSystemMock
           .Expect (mock => mock.FileExists (_assemblyDefinition1.Modules[1].FullyQualifiedName)).Return (true);
      _fileSystemMock
           .Expect (mock => mock.FileExists (_assemblyDefinition1.Modules[1].FullyQualifiedName + ".bak0")).Return (false);
      _fileSystemMock
          .Expect (mock => mock.Move (_assemblyDefinition1.Modules[1].FullyQualifiedName, _assemblyDefinition1.Modules[1].FullyQualifiedName + ".bak0"));
      _fileSystemMock
          .Expect (mock => mock.WriteModuleDefinition (
            Arg<ModuleDefinition>.Is.Same (_assemblyDefinition1.Modules[1]),
            Arg<string>.Is.Same (_assemblyDefinition1.Modules[1].FullyQualifiedName),
            Arg<WriterParameters>.Matches (param => param.StrongNameKeyPair == null)));
      _fileSystemMock.Replay ();

      _definitionWriter.WriteModule (_assemblyDefinition1.Modules[1]);

      _fileSystemMock.VerifyAllExpectations ();
    }
  }
}