// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.IO;
using System.Reflection;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;

namespace AssemblyTransformer.UnitTests.AssemblySigning
{
  [TestFixture]
  public class AssemblySignerFactoryTest
  {
    private IFileSystem _fileSystemMock;
    private AssemblySignerFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem> ();
      _factory = new AssemblySignerFactory (_fileSystemMock);
    }

    // TODO Review FS: If easily possible, add a test for AddOptions that shows that the "-k", "--key", "-s", "--keyDir", "--keyDirectories" options are added
    // TODO Review FS: (Also do this for the other factory tests.)

    [Test]
    public void CreateSigner_NoOptions ()
    {
      _fileSystemMock.Replay();
      var result = _factory.CreateSigner ();

      _fileSystemMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf (typeof (AssemblySigner)));
      // TODO Review FS: Check the default key and available key pairs of the writer - this will probably require adding a Writer property on AssemblySigner, and DefaultKey and AvailableKeys properties on ModuleDefinitionWriter
    }

    // TODO Review FS: Try to write tests successfully getting and using a default key and key set (without expecting an ArgumentException). To do so:
    // TODO Review FS: - change IFileSystem.Open to return Stream instead of FileStream
    // TODO Review FS: - in the expectation (Expect (mock => mock.Open (...))), return an in-memory stream containing a valid key blob (add an object mother class for this)
    // TODO Review FS: In those tests, check that the signer's writer has the right default and strong name keys

    [Test]
    [ExpectedException (typeof(ArgumentNullException))]
    public void CreateSigner_WithDefaultKey ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "-k:someKey" });

      _fileSystemMock
          .Expect (mock => mock.Open("someKey", FileMode.Open))
          .Return (null);
      _fileSystemMock.Replay ();

      var result = _factory.CreateSigner ();

      _fileSystemMock.VerifyAllExpectations ();
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void CreateSigner_WithKeySet ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "-s:someDir" });

      _fileSystemMock
          .Expect (mock => mock.EnumerateFiles ("someDir", "*.snk", SearchOption.AllDirectories))
          .Return (new[] { @"something\1.snk" });
      _fileSystemMock
          .Expect (mock => mock.Open (@"something\1.snk", FileMode.Open))
          .Return (null);

      _fileSystemMock.Replay ();

      var result = _factory.CreateSigner ();

      _fileSystemMock.VerifyAllExpectations ();
    }
  }
}