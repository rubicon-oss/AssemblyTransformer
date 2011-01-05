// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Reflection;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblySigning.AssemblyWriting;
using AssemblyTransformer.AssemblyTransformations.AssemblyMethodsVirtualizing.MarkingStrategies;
using Mono.Cecil;
using System.Collections.Generic;
using NUnit.Framework;

namespace AssemblyTransformer.UnitTests.AssemblyMarking
{
  [TestFixture]
  public class CustomMarkingAttributeStrategyTest
  {
    private AssemblyDefinition _assemblyDefinition;
    private IMarkingAttributeStrategy _markerCustomMarkingStrategy;

    [SetUp]
    public void SetUp ()
    {
      _assemblyDefinition = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition ();
      _markerCustomMarkingStrategy = new CustomMarkingAttributeStrategy (
          "CustomNonVirtualAttribute", "CustomNonVirtualAttribute", ModuleDefinition.ReadModule ("CecilUser.exe")
        );
    }

    [TearDown]
    public void TearDown ()
    {
      IModuleDefinitionWriter writer = new ModuleDefinitionWriter (new FileSystem.FileSystem (), null, new List<StrongNameKeyPair> ());
      foreach (var module in _assemblyDefinition.Modules)
      {
        Console.WriteLine ("###### " + module.Name);
       // writer.WriteModule (_assemblyDefinition.MainModule, module);
      }
    }

    [Test]
    public void OverrideMethods_MainModule_MethodMarked ()
    {
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];
      ModuleDefinition attributeModule = ModuleDefinition.ReadModule ("CecilUser.exe");

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _markerCustomMarkingStrategy.AddCustomAttribute (methodMain, _assemblyDefinition);

      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes[0].AttributeType.Name, Is.EqualTo ("CustomNonVirtualAttribute"));

      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences.Count, Is.EqualTo (0));
      Assert.That (_assemblyDefinition.Modules[1].Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (0));

      TypeReference attributeType = attributeModule.Types[1];
      Assert.That (attributeType.Scope.MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleDefinition));
    }

    [Test]
    public void OverrideMethods_SecondaryModule_MethodMarked ()
    {
      MethodDefinition secondModuleMethod = _assemblyDefinition.Modules[1].Types[1].Methods[0];
      ModuleDefinition attributeModule = ModuleDefinition.ReadModule ("CecilUser.exe");

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _markerCustomMarkingStrategy.AddCustomAttribute (secondModuleMethod, _assemblyDefinition);

      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (0));

      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences[0].Name, Is.EqualTo (attributeModule.Name));
      Assert.That (_assemblyDefinition.Modules[1].Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.Modules[1].Types[1].Methods[0].CustomAttributes[0].AttributeType.Name, Is.EqualTo ("CustomNonVirtualAttribute"));

      TypeReference attributeType = attributeModule.Types[1];
      Assert.That (attributeType.Scope.MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleDefinition));
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences[0].MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleReference));

    }

    [Test]
    public void OverrideMethods_BothModules_MethodsMarked ()
    {
      MethodDefinition mainModuleMethod = _assemblyDefinition.MainModule.Types[1].Methods[0];
      MethodDefinition secondModuleMethod = _assemblyDefinition.Modules[1].Types[1].Methods[0];
      ModuleDefinition attributeModule = ModuleDefinition.ReadModule ("CecilUser.exe");

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _markerCustomMarkingStrategy.AddCustomAttribute (mainModuleMethod, _assemblyDefinition);
      _markerCustomMarkingStrategy.AddCustomAttribute (secondModuleMethod, _assemblyDefinition);

      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes[0].AttributeType.Name, Is.EqualTo ("CustomNonVirtualAttribute"));

      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences[0].Name, Is.EqualTo (attributeModule.Name));
      Assert.That (_assemblyDefinition.Modules[1].Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.Modules[1].Types[1].Methods[0].CustomAttributes[0].AttributeType.Name, Is.EqualTo ("CustomNonVirtualAttribute"));

      TypeReference attributeType = attributeModule.Types[1];
      Assert.That (attributeType.Scope.MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleDefinition));
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences[0].MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleReference));

    }

    [Test]
    public void OverrideMethods_AttributeTypeNotFound_Exception ()
    {
      _markerCustomMarkingStrategy = new CustomMarkingAttributeStrategy (
          "NotFoundAttribute", "NotFoundAttribute", ModuleDefinition.ReadModule ("CecilUser.exe")
        );
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];
      ModuleDefinition attributeModule = ModuleDefinition.ReadModule ("CecilUser.exe");

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      Assert.Throws<InvalidOperationException> (() => _markerCustomMarkingStrategy.AddCustomAttribute (methodMain, _assemblyDefinition));
    }

  }
}