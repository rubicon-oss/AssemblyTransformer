// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Reflection;
using AssemblyMethodsVirtualizer.UnitTests;
using AssemblyTransformer.Extensions;
using ConstructorGenerator.MixinChecker;
using Mono.Cecil;
using NUnit.Framework;

namespace ConstructorGeneratorUnitTests.MixinCheckerTest
{
  [TestFixture]
  public class StringAndReflectionBasedMixinCheckerTest
  {

    private AssemblyDefinition _assemblyDefinition;
    StringAndReflectionBasedMixinChecker _checker;

    [SetUp]
    public void SetUp ()
    {
      _assemblyDefinition = AssemblyDefinitionObjectMother.CreateAssemblyDefinition();
    }

    [Test]
    [ExpectedException(typeof(TargetInvocationException))]
    public void MixinChecker_Exception ()
    {

      _checker = new StringAndReflectionBasedMixinChecker ("workingDirectory", "assemblyNameRemotionInterfaces");

      var result = _checker.CanBeMixed (_assemblyDefinition.Name.BuildAssemblyQualifiedName(_assemblyDefinition.MainModule.Types[0]));

      Assert.That (result, Is.False);
    }

  }
}