// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq;
using AssemblyMethodsVirtualizer.UnitTests;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.FileSystem;
using ConstructorGenerator.CodeGenerator;
using ConstructorGenerator.ReferenceGenerator;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;

namespace ConstructorGeneratorUnitTests.MethodReferenceGeneratorTest
{
  [TestFixture]
  public class MethodReferenceGeneratorTest
  {
    private IFileSystem _fileSystemMock;
    private IAssemblyTracker _tracker;
    private AssemblyDefinition _assemblyDefinition1;

    private ICodeGenerator _codeGenerator;
    private MethodReferenceGenerator _referenceGenerator;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem> ();
      _assemblyDefinition1 = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyWithCtorsAndMethodBody ();
      _tracker = MockRepository.GenerateStrictMock<IAssemblyTracker> ();
    }

    [Test]
    public void ReferenceGenerator_CreatesObjectFactoryReference ()
    {
      _tracker.Stub (mock => mock.TrackNewReference (Arg<AssemblyDefinition>.Is.Equal (_assemblyDefinition1), Arg<AssemblyNameReference>.Is.Anything));
      _referenceGenerator =
          new MethodReferenceGenerator (
              "Remotion.Interfaces, Version=1.13.73.1026, Culture=neutral, PublicKeyToken=fee00910d6e5f53b", "Factory.ObjectFactory", "Param.ParamList");

      var result = _referenceGenerator.GetCallableObjectFactoryCreateMethod (
          _assemblyDefinition1, _assemblyDefinition1.MainModule, _assemblyDefinition1.MainModule.Types[1], _tracker);

      Assert.That (result != null);
      Assert.That (_assemblyDefinition1.MainModule.AssemblyReferences.FirstOrDefault (r => r.FullName == "Remotion.Interfaces, Version=1.13.73.1026, Culture=neutral, PublicKeyToken=fee00910d6e5f53b") != null);
    }

    [Test]
    public void ReferenceGenerator_InitializesNamesAndReference ()
    {
      System.Text.UnicodeEncoding enc = new System.Text.UnicodeEncoding();
      _referenceGenerator =
        new MethodReferenceGenerator (
          "Remotion.Interfaces, Version=1.13.73.1026, Culture=neutral, PublicKeyToken=fee00910d6e5f53b", "Factory.ObjectFactory", "Param.ParamList");

      Assert.That (_referenceGenerator.ObjectFactoryName == "ObjectFactory");
      Assert.That (_referenceGenerator.ObjectFactoryNamespace == "Factory");
      Assert.That (_referenceGenerator.ParamListName == "ParamList");
      Assert.That (_referenceGenerator.ParamListNamespace == "Param");
    }

    [Test]
    public void ReferenceGenerator_CreatesParamListReference ()
    {
      _tracker.Stub (mock => mock.TrackNewReference (Arg<AssemblyDefinition>.Is.Equal (_assemblyDefinition1), Arg<AssemblyNameReference>.Is.Anything));
      _referenceGenerator =
          new MethodReferenceGenerator (
              "Remotion.Interfaces, Version=1.13.73.1026, Culture=neutral, PublicKeyToken=fee00910d6e5f53b", "Factory.ObjectFactory", "Param.ParamList");


      var result = _referenceGenerator.GetCallableParamListCreateMethod (_assemblyDefinition1 ,_assemblyDefinition1.MainModule.Types[1].Methods[1], _tracker);

      Assert.That (result != null);
      Assert.That (result.IsGenericInstance);
      Assert.That (result.Parameters.Count == 1);
      Assert.That (((GenericInstanceMethod) result).GenericArguments.Count == 1);
      Assert.That (((GenericInstanceMethod) result).GenericArguments[0].GetElementType() == _assemblyDefinition1.MainModule.TypeSystem.String);
    }

    [Test]
    public void ReferenceGenerator_GetOrCreateParamList_createsReference ()
    {
      _referenceGenerator =
          new MethodReferenceGenerator (
              "Remotion.Interfaces, Version=1.13.73.1026, Culture=neutral, PublicKeyToken=fee00910d6e5f53b", "Factory.ObjectFactory", "Param.ParamList");
      
      var reference = _referenceGenerator.GetOrCreateParamList (1, _assemblyDefinition1.Name);

      Assert.That (reference.Parameters.Count == 1);
      Assert.That (reference.GenericParameters.Count == 1);
      Assert.That (reference.ReturnType.Name == "ParamList");
    }

  }
}