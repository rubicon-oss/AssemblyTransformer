// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyMethodsVirtualizer.UnitTests;
using AssemblyTransformer;
using AssemblyTransformer.FileSystem;
using ConstructorGenerator;
using ConstructorGenerator.CodeGenerator;
using ConstructorGenerator.MixinChecker;
using ConstructorGenerator.ReferenceGenerator;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;

namespace ConstructorGeneratorUnitTests.ConstructorGeneratorTest
{
  [TestFixture]
  public class ConstructorGeneratorFactoryTest
  {
    private IFileSystem _fileSystemMock;
    private ConstructorGeneratorFactory _factory;

    private AssemblyDefinition _assemblyDefinition1;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem> ();
      _factory = new ConstructorGeneratorFactory (_fileSystemMock);

      _assemblyDefinition1 = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition ();
    }

    [Test]
    public void CreateTransformation_ParsesParameters ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "--workingDir=.", "--fac=namespace.Factory", "--par=namespace.Param", "--rem=Remotion" });

      var result = _factory.CreateTransformation ();

      Assert.That (result, Is.TypeOf (typeof (ConstructorGenerator.ConstructorGenerator)));
      Assert.That (((StringAndReflectionBasedMixinChecker)((ConstructorGenerator.ConstructorGenerator) result).Checker) != null);
      Assert.That (((MethodReferenceGenerator) ((ILCodeGenerator) 
        ((ConstructorGenerator.ConstructorGenerator) result).CodeGenerator).ReferenceGenerator).ObjectFactoryName == "Factory");
      Assert.That (((MethodReferenceGenerator) ((ILCodeGenerator) 
        ((ConstructorGenerator.ConstructorGenerator) result).CodeGenerator).ReferenceGenerator).ObjectFactoryNamespace == "namespace");
      Assert.That (((MethodReferenceGenerator) ((ILCodeGenerator) 
        ((ConstructorGenerator.ConstructorGenerator) result).CodeGenerator).ReferenceGenerator).ParamListNamespace == "namespace");
      Assert.That (((MethodReferenceGenerator) ((ILCodeGenerator) 
        ((ConstructorGenerator.ConstructorGenerator) result).CodeGenerator).ReferenceGenerator).ParamListName == "Param");

    }

  }
}