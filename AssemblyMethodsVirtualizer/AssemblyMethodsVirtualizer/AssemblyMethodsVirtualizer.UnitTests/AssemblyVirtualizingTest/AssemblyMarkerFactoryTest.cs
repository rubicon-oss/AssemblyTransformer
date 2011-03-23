// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyMethodsVirtualizer.MarkingStrategies;
using AssemblyTransformer;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;

namespace AssemblyMethodsVirtualizer.UnitTests.AssemblyVirtualizingTest
{
  [TestFixture]
  public class AssemblyMarkerFactoryTest
  {
    private IFileSystem _fileSystemMock;
    private AssemblyMethodVirtualizerFactory _factory;

    private AssemblyDefinition _assemblyDefinition1;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem> ();
      _factory = new AssemblyMethodVirtualizerFactory (_fileSystemMock);

      _assemblyDefinition1 = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void CreateTransformation_NotInitialized ()
    {
      _factory.CreateTransformation();
    }

    [Test]
    public void CreateTransformation_GeneratedStrategy ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "--regex:regex", "--att:Generated", "--attNS:attNS", "--attType:attName", "--attFile:attFile" });

      var result = _factory.CreateTransformation ();
      
      Assert.That (result, Is.TypeOf (typeof (AssemblyMethodsVirtualizer)));
      Assert.That (((AssemblyMethodsVirtualizer) result).MarkingAttributeStrategy, Is.TypeOf (typeof (GeneratedMarkingAttributeStrategy)));
      Assert.That (((AssemblyMethodsVirtualizer) result).TargetMethodsFullNameMatchingRegex.ToString () == "regex");
    }

    [Test]
    public void CreateTransformation_CreatesCustomStrategy ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "--regex:regex", "--att:Custom", "--attNS:TestSpace", "--attType:TestType", "--attFile:attFile" });
      _fileSystemMock
          .Expect (mock => mock.ReadAssembly("attFile"))
          .Return (_assemblyDefinition1);
      _fileSystemMock.Replay();

      var result = _factory.CreateTransformation ();

      Assert.That (result, Is.TypeOf (typeof (AssemblyMethodsVirtualizer)));
      _fileSystemMock.VerifyAllExpectations();
      Assert.That (((AssemblyMethodsVirtualizer) result).MarkingAttributeStrategy, Is.TypeOf (typeof (CustomMarkingAttributeStrategy)));
      Assert.That (((AssemblyMethodsVirtualizer) result).TargetMethodsFullNameMatchingRegex.ToString () == "regex");
      Assert.That (((CustomMarkingAttributeStrategy) ((AssemblyMethodsVirtualizer) result).MarkingAttributeStrategy).AttributeName == "TestType");
      Assert.That (((CustomMarkingAttributeStrategy) ((AssemblyMethodsVirtualizer) result).MarkingAttributeStrategy).AttributeNamespace == "TestSpace");
      Assert.That (((CustomMarkingAttributeStrategy) ((AssemblyMethodsVirtualizer) result).MarkingAttributeStrategy).ModuleContainingAttribute == _assemblyDefinition1.MainModule);
    }

    [Test]
    public void CreateTransformation_NoneStrategy ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "--regex:regex", "--att:None", "--attNS:attNS", "--attType:attName", "--attFile:attFile" });

      var result = _factory.CreateTransformation ();

      Assert.That (result, Is.TypeOf (typeof (AssemblyMethodsVirtualizer)));
      Assert.That (((AssemblyMethodsVirtualizer) result).MarkingAttributeStrategy, Is.TypeOf (typeof (NoneMarkingAttributeStrategy)));
      Assert.That (((AssemblyMethodsVirtualizer) result).TargetMethodsFullNameMatchingRegex.ToString () == "regex");
    }

    [Test]
    public void CreateTransformation_NoneStrategyIsDefault ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "-regex:regex" });

      var result = _factory.CreateTransformation ();

      Assert.That (result, Is.TypeOf (typeof (AssemblyMethodsVirtualizer)));
      Assert.That (((AssemblyMethodsVirtualizer) result).MarkingAttributeStrategy, Is.TypeOf (typeof (NoneMarkingAttributeStrategy)));
      Assert.That (((AssemblyMethodsVirtualizer) result).TargetMethodsFullNameMatchingRegex.ToString () == "regex");
    }
  }
}