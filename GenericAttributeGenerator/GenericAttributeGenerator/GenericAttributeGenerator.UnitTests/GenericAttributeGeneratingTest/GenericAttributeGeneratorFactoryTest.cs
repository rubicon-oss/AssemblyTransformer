// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer;
using AssemblyTransformer.FileSystem;
using NUnit.Framework;
using Rhino.Mocks;

namespace GenericAttributeGenerator.UnitTests.GenericAttributeGeneratingTest
{
  [TestFixture]
  public class GenericAttributeGeneratorFactoryTest
  {
    private IFileSystem _fileSystemMock;

    private GenericAttributeGeneratorFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem> ();
      _factory = new GenericAttributeGeneratorFactory(_fileSystemMock);
    }


    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void CreateTransformation_NotInitialized ()
    {
      _factory.CreateTransformation ();
    }

    [Test]
    public void CreateTransformation ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "-d:workingDir" });

      var result = _factory.CreateTransformation ();

      Assert.That (result, Is.TypeOf (typeof (GenericAttributeGenerator)));
      Assert.That (((GenericAttributeGenerator) result).GenericMarker == typeof(GenericAttributeMarkerAttribute));
    }
    
  }
}