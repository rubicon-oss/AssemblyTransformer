// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.IO;
using System.Reflection;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblySigning.AssemblyWriting;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;
using System.Linq;

namespace AssemblyTransformer.UnitTests.AssemblySigning
{
  [TestFixture]
  public class AssemblySignerFactoryTest
  {
    private IFileSystem _fileSystemMock;
    private AssemblySignerFactory _factory;
    private OptionSet _options;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem> ();
      _factory = new AssemblySignerFactory (_fileSystemMock);
      //_options = MockRepository.GenerateStrictMock <OptionSet> ();
      _options = new OptionSet ();
    }

    [Test]
    public void CreateSigner_AddsOptions ()
    {
      //_options.Expect (
      //    mock => mock.Add (
      //        Arg<string>.Is.Equal ("k|key|defaultKey="),
      //        Arg<string>.Is.Equal ("The default key (.snk) to be used to sign Assemblies."),
      //        Arg<Action<string>>.Is.NotNull));
      //_options.Expect (
      //    mock => mock.Add (
      //        Arg<string>.Is.Equal ("s|keyDir|keyDirectory="),
      //        Arg<string>.Is.Equal ("The root dir of all keys (.snk) to sign Assemblies."),
      //        Arg<Action<string>>.Is.NotNull));

      _factory.AddOptions (_options);

      Assert.That (_options.Contains ("k"));
      Assert.That (_options.Contains ("defaultKey"));
      Assert.That (_options.Contains ("y"));
      Assert.That (_options.Contains ("keyDir"));
    }

    [Test]
    public void CreateSigner_NoOptions ()
    {
      _fileSystemMock.Replay();
      var result = _factory.CreateSigner ();

      _fileSystemMock.VerifyAllExpectations();
      Assert.That (result, Is.TypeOf (typeof (AssemblySigner)));
      Assert.That (((ModuleDefinitionWriter) ((AssemblySigner) result).Writer).DefaultKey == null);
      Assert.That (((ModuleDefinitionWriter) ((AssemblySigner) result).Writer).Keys.Length == 0);
    }

    [Test]
    public void CreateSigner_HasCorrectKeys ()
    {
      var key = AssemblyNameReferenceObjectMother.RealKeyPairStream ();
      var keyPair = AssemblyNameReferenceObjectMother.RealKeyPair ();

      _factory.AddOptions (_options);
      _options.Parse (new[] { "-k:someKey", "-y:someDir" });
      
      _fileSystemMock
          .Expect (mock => mock.Open ("someKey", FileMode.Open))
          .Return (AssemblyNameReferenceObjectMother.RealKeyPairStream ());
      _fileSystemMock
          .Expect (mock => mock.EnumerateFiles ("someDir", "*.snk", SearchOption.AllDirectories))
          .Return (new[] { @"something\1.snk" });
      _fileSystemMock
          .Expect (mock => mock.Open (@"something\1.snk", FileMode.Open))
          .Return (AssemblyNameReferenceObjectMother.RealKeyPairStream ());
      _fileSystemMock.Replay ();

      var result = _factory.CreateSigner ();

      _fileSystemMock.VerifyAllExpectations ();
      Assert.That (result, Is.TypeOf (typeof (AssemblySigner)));
      Assert.That (((ModuleDefinitionWriter) ((AssemblySigner) result).Writer).DefaultKey.PublicKey.SequenceEqual (AssemblyNameReferenceObjectMother.RealKeyPair().PublicKey));
      Assert.That (((ModuleDefinitionWriter) ((AssemblySigner) result).Writer).Keys.Length == 1);
      Assert.That (((ModuleDefinitionWriter) ((AssemblySigner) result).Writer).Keys[0].PublicKey.SequenceEqual (AssemblyNameReferenceObjectMother.RealKeyPair ().PublicKey));
    }

    [Test]
    [ExpectedException (typeof(ArgumentNullException))]
    public void CreateSigner_WithDefaultKey ()
    {
      _factory.AddOptions (_options);
      _options.Parse (new[] { "-k:someKey" });

      _fileSystemMock
          .Expect (mock => mock.Open("someKey", FileMode.Open))
          .Return (null);

      var result = _factory.CreateSigner ();

      _fileSystemMock.VerifyAllExpectations ();     
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void CreateSigner_WithKeySet ()
    {
      _factory.AddOptions (_options);
      _options.Parse (new[] { "-y:someDir" });

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