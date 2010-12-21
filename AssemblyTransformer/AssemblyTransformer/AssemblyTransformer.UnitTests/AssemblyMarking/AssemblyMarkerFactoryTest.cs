// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.IO;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations.AssemblyMarking;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;
using Mono.Options;
using NUnit.Framework;
using Rhino.Mocks;

namespace AssemblyTransformer.UnitTests.AssemblyMarking
{
  [TestFixture]
  public class AssemblyMarkerFactoryTest
  {
    private IFileSystem _fileSystemMock;
    private AssemblyMarkerFactory _factory;

    private AssemblyDefinition _assemblyDefinition1;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem> ();
      _factory = new AssemblyMarkerFactory (_fileSystemMock);

      _assemblyDefinition1 = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void CreateTransformation_NotInitialized ()
    {
      _factory.CreateTransformation();
    }

    [Test]
    public void CreateTransformation ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "-r:regex", "-a:Default", "-n:attNS", "-t:attName", "-f:attFile" });

      var result = _factory.CreateTransformation ();

      Assert.That (result, Is.TypeOf (typeof (AssemblyMarker)));
    }

    [Test]
    public void CreateTransformation_CreatesCustomStrategy ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "-r:regex", "-a:Custom", "-n:TestSpace", "-t:TestType", "-f:attFile" });
      _fileSystemMock
          .Expect (mock => mock.ReadAssembly("attFile"))
          .Return (_assemblyDefinition1);
      _fileSystemMock.Replay();

      var result = _factory.CreateTransformation ();

      Assert.That (result, Is.TypeOf (typeof (AssemblyMarker)));
      _fileSystemMock.VerifyAllExpectations();
    }
  }
}