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
using Mono.Options;
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

    [Test]
    public void CreateSigner_NoOptions ()
    {
      _fileSystemMock.Replay();
      var result = _factory.CreateSigner ();

      _fileSystemMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf (typeof (AssemblySigner)));
    }

    [Test]
    [ExpectedException (typeof(ArgumentException))]
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
    [ExpectedException (typeof (ArgumentException))]
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