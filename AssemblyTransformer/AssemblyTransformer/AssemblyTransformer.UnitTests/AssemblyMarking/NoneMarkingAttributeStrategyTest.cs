// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer.AssemblyMarking;
using AssemblyTransformer.AssemblyMarking.MarkingStrategies;
using Mono.Cecil;
using NUnit.Framework;

namespace AssemblyTransformer.UnitTests.AssemblyMarking
{
  [TestFixture]
  public class NoneMarkingAttributeStrategyTest
  {
    private AssemblyDefinition _assemblyDefinition;

    private IMarkingAttributeStrategy _noAttributeMarkingStrategy;

    [SetUp]
    public void SetUp ()
    {
      _assemblyDefinition = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition ();
      _noAttributeMarkingStrategy = new NoneMarkingAttributeStrategy();
    }


    [Test]
    public void OverrideMethods_BothMethodsMarked_NoAttribute ()
    {
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];
      MethodDefinition methodModule = _assemblyDefinition.Modules[1].Types[1].Methods[0];

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);

      _noAttributeMarkingStrategy.AddCustomAttribute(methodMain, methodMain.DeclaringType.Module);
      _noAttributeMarkingStrategy.AddCustomAttribute (methodModule, methodMain.DeclaringType.Module);

      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.Modules[1].Types.Count, Is.EqualTo (2));
      Assert.That (methodMain.CustomAttributes, Is.Empty);
      Assert.That (methodModule.CustomAttributes, Is.Empty);
    }

    [Test]
    public void OverrideMethods_MainModule_MethodMarked_NoAttribute ()
    {
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];
      MethodDefinition methodModule = _assemblyDefinition.Modules[1].Types[1].Methods[0];

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);

      _noAttributeMarkingStrategy.AddCustomAttribute (methodMain, methodMain.DeclaringType.Module);
      
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.Modules[1].Types.Count, Is.EqualTo (2));
      Assert.That (methodMain.CustomAttributes, Is.Empty);
      Assert.That (methodModule.CustomAttributes, Is.Empty);
    }

    [Test]
    public void OverrideMethods_SecondaryModule_MethodMarked_NoAttribute ()
    {
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];
      MethodDefinition methodModule = _assemblyDefinition.Modules[1].Types[1].Methods[0];

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);

      _noAttributeMarkingStrategy.AddCustomAttribute (methodModule, methodMain.DeclaringType.Module);

      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.Modules[1].Types.Count, Is.EqualTo (2));
      Assert.That (methodMain.CustomAttributes, Is.Empty);
      Assert.That (methodModule.CustomAttributes, Is.Empty);
    }
  }
}