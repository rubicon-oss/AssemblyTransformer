// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer.AssemblyTransformations.AssemblyMethodsVirtualizing;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;
using AssemblyTransformer.AssemblyTransformations.AssemblyMethodsVirtualizing.MarkingStrategies;

namespace AssemblyTransformer.UnitTests.AssemblyMarking
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
      optionSet.Parse (new[] { "-r:regex", "-a:Generated", "-n:attNS", "-t:attName", "-f:attFile" });

      var result = _factory.CreateTransformation ();
      
      Assert.That (result, Is.TypeOf (typeof (AssemblyMethodsVirtualizer)));
      Assert.That (((AssemblyMethodsVirtualizer)result).MarkingAttributeStrategy, Is.TypeOf (typeof(GeneratedMarkingAttributeStrategy)));
      Assert.That (((AssemblyMethodsVirtualizer) result).TargetMethodsFullNameMatchingRegex.ToString() == "regex");
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

      Assert.That (result, Is.TypeOf (typeof (AssemblyMethodsVirtualizer)));
      _fileSystemMock.VerifyAllExpectations();

      // TODO Review FS: Check that the CustomMarkingStrategy is used
      // TODO Review FS: Check that the CustomMarkingStrategy uses the right module and type namespace/name
    }

    [Test]
    public void CreateTransformation_NoneStrategy ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "-r:regex", "-a:None", "-n:attNS", "-t:attName", "-f:attFile" });

      var result = _factory.CreateTransformation ();

      Assert.That (result, Is.TypeOf (typeof (AssemblyMethodsVirtualizer)));
      // TODO Review FS: Check that the DefaultMarkingStrategy is used (probably requires a MarkingStrategy property on AssemblyMethodsVirtualizer)
      // TODO Review FS: Check the regex (probably requires a Regex property on AssemblyMethodsVirtualizer)
    }

    [Test]
    public void CreateTransformation_NoneStrategyIsDefault ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "-r:regex" });

      var result = _factory.CreateTransformation ();

      Assert.That (result, Is.TypeOf (typeof (AssemblyMethodsVirtualizer)));
      // TODO Review FS: Check that the DefaultMarkingStrategy is used (probably requires a MarkingStrategy property on AssemblyMethodsVirtualizer)
      // TODO Review FS: Check the regex (probably requires a Regex property on AssemblyMethodsVirtualizer)
    }
    // TODO Review FS: Add a test showing that None is the default strategy if no "-a:..." is given (check that NoneMarkingAttributeStrategy is used when constructing the AssemblyMethodsVirtualizer)
  }
}