// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.IO;
using System.Reflection;
using AssemblyMethodsVirtualizer;
using AssemblyTransformer.AssemblyTransformations.AssemblyTransformationFactoryFactory;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;

namespace AssemblyTransformer.UnitTests.TransformationFactoryFactoryTest
{
  [TestFixture]
  public class DLLBasedTransformationFactoryFactoryTest
  {

    private IFileSystem _fileSystemMock;
    private DLLBasedTransformationFactoryFactory _factory;

    private Assembly _assembly1 = Assembly.GetCallingAssembly();

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem> ();
      _factory = new DLLBasedTransformationFactoryFactory (_fileSystemMock);

      
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void CreateTransformationFactories_NotInitialized ()
    {
      _factory.CreateTrackerFactories();
    }

    [Test]
    public void CreateTransformationFactories ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      _factory.WorkingDirectory = ".";
      optionSet.Parse (new []{"-t=testcase"});

      _fileSystemMock.Expect (mock => mock.LoadAssemblyFrom (@".\testcase.dll")).Return (_assembly1);
      _fileSystemMock.Replay ();

      var result = _factory.CreateTrackerFactories();
      _fileSystemMock.VerifyAllExpectations ();

    }

    [Test]
    public void CreateTransformationFactories_WithRealAssembly ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      _factory.WorkingDirectory = ".";
      optionSet.Parse (new[] { "-t=testcase"});

      _fileSystemMock.Expect (mock => mock.LoadAssemblyFrom (@".\testcase.dll")).
        Return (System.Reflection.Assembly.LoadFrom (Path.Combine (AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\prereq\testing\transformation\AssemblyMethodsVirtualizer.dll")));
      _fileSystemMock.Replay ();

      var result = _factory.CreateTrackerFactories ();
      _fileSystemMock.VerifyAllExpectations ();

      foreach (var factory in result)
      {
        Console.WriteLine (factory.GetType ());
        Assert.That (factory.GetType().FullName == typeof(AssemblyMethodVirtualizerFactory).FullName);
      }

    }

  }
}