// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyMethodsVirtualizer.MarkingStrategies;
using Mono.Cecil;
using NUnit.Framework;

namespace AssemblyMethodsVirtualizer.UnitTests.AssemblyVirtualizingTest
{
  [TestFixture]
  public class DefaultMarkingAttributeStrategyTest
  {
    private AssemblyDefinition _assemblyDefinition;
    private IMarkingAttributeStrategy _markerDefaultMarkingStrategy;

    [SetUp]
    public void SetUp ()
    {
      _assemblyDefinition = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition ();
      _markerDefaultMarkingStrategy = new GeneratedMarkingAttributeStrategy ("NonVirtualAttribute", "NonVirtualAttribute");
    }

    [Test]
    public void OverrideMethods_MainModule_MethodMarked ()
    {
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _markerDefaultMarkingStrategy.AddCustomAttribute (methodMain, _assemblyDefinition);

      // Main module was modified
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (3));
      Assert.That (_assemblyDefinition.MainModule.Types[2].Name, Is.EqualTo ("NonVirtualAttribute"));
      Assert.That (methodMain.CustomAttributes.Count, Is.EqualTo (1));
      Assert.That (methodMain.CustomAttributes[0].AttributeType.Name, Is.EqualTo ("NonVirtualAttribute"));
      Assert.That (methodMain.CustomAttributes[0].AttributeType, Is.SameAs (_assemblyDefinition.MainModule.Types[2]));

      // Second module was not touched
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences.Count, Is.EqualTo (0));
      Assert.That (_assemblyDefinition.Modules[1].Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (0));

      TypeReference attributeType = _assemblyDefinition.MainModule.Types[2];
      Assert.That (attributeType.Scope.MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleDefinition));
    }

    [Test]
    public void OverrideMethods_MainModule_MethodMarkedTwice ()
    {
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _markerDefaultMarkingStrategy.AddCustomAttribute (methodMain, _assemblyDefinition);
      _markerDefaultMarkingStrategy.AddCustomAttribute (methodMain, _assemblyDefinition);

      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (3));
      Assert.That (_assemblyDefinition.MainModule.Types[2].Name, Is.EqualTo ("NonVirtualAttribute"));
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (1));
    }


    [Test]
    public void OverrideMethods_SecondaryModule_MethodMarked ()
    {
      MethodDefinition secondModuleMethod = _assemblyDefinition.Modules[1].Types[1].Methods[0];
      ModuleDefinition mainModule = _assemblyDefinition.MainModule;

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _markerDefaultMarkingStrategy.AddCustomAttribute (secondModuleMethod, _assemblyDefinition);

      // Main module contains attribute type, but method not modified
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (3));
      Assert.That (_assemblyDefinition.MainModule.Types[2].Name, Is.EqualTo ("NonVirtualAttribute"));
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (0));

      // Second module was modified
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences[0].Name, Is.EqualTo (_assemblyDefinition.MainModule.Name));
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences[0].MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleReference));
      Assert.That (secondModuleMethod.CustomAttributes.Count, Is.EqualTo (1));
      Assert.That (secondModuleMethod.CustomAttributes[0].AttributeType.Name, Is.EqualTo ("NonVirtualAttribute"));       // ModuleReferences!!
      Assert.That (secondModuleMethod.CustomAttributes[0].AttributeType.Scope, Is.SameAs (_assemblyDefinition.Modules[1].AssemblyReferences[1]));

      TypeReference attributeType = _assemblyDefinition.MainModule.Types[2];
      Assert.That (attributeType.Scope.MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleDefinition));
    }

    [Test]
    public void OverrideMethods_BothModules_MethodsMarked ()
    {
      MethodDefinition mainModuleMethod = _assemblyDefinition.MainModule.Types[1].Methods[0];
      MethodDefinition secondModuleMethod = _assemblyDefinition.Modules[1].Types[1].Methods[0];
      ModuleDefinition mainModule = _assemblyDefinition.MainModule;

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _markerDefaultMarkingStrategy.AddCustomAttribute (mainModuleMethod, _assemblyDefinition);
      _markerDefaultMarkingStrategy.AddCustomAttribute (secondModuleMethod, _assemblyDefinition);

      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (3));
      Assert.That (_assemblyDefinition.MainModule.Types[2].Name, Is.EqualTo ("NonVirtualAttribute"));
      Assert.That (mainModuleMethod.CustomAttributes.Count, Is.EqualTo (1));
      Assert.That (mainModuleMethod.CustomAttributes[0].AttributeType.Name, Is.EqualTo ("NonVirtualAttribute"));

      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences[0].Name, Is.EqualTo (_assemblyDefinition.MainModule.Name));
      Assert.That (secondModuleMethod.CustomAttributes.Count, Is.EqualTo (1));
      Assert.That (secondModuleMethod.CustomAttributes[0].AttributeType.Name, Is.EqualTo ("NonVirtualAttribute"));

      TypeReference attributeType = _assemblyDefinition.MainModule.Types[2];
      Assert.That (attributeType.Scope.MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleDefinition));
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences[0].MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleReference));
    }
  }
}