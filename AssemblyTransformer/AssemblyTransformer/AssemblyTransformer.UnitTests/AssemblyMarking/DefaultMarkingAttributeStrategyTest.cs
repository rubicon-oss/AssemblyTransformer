// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer.AssemblyMarking;
using AssemblyTransformer.AssemblyMarking.MarkingStrategies;
using AssemblyTransformer.AssemblySigning;
using Mono.Cecil;
using NUnit.Framework;

namespace AssemblyTransformer.UnitTests.AssemblyMarking
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
      _markerDefaultMarkingStrategy = new DefaultMarkingAttributeStrategy ("NonVirtualAttribute", "NonVirtualAttribute");
    }


    [Test]
    public void OverrideMethods_MainModule_MethodMarked ()
    {
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _markerDefaultMarkingStrategy.AddCustomAttribute (methodMain, methodMain.DeclaringType.Module);

      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (3));
      Assert.That (_assemblyDefinition.MainModule.Types[2].Name, Is.EqualTo ("NonVirtualAttribute"));
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes[0].AttributeType.Name, Is.EqualTo ("NonVirtualAttribute"));

      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences.Count, Is.EqualTo (0));
      Assert.That (_assemblyDefinition.Modules[1].Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (0));

      TypeReference attributeType = _assemblyDefinition.MainModule.Types[2];
      Assert.That (attributeType.Scope.MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleDefinition));

      IModuleDefinitionWriter writer = new AssemblyTransformer.AssemblyWriting.ModuleWriter ();
      foreach (var module in _assemblyDefinition.Modules)
      {
        Console.WriteLine ("###### " + module.Name);
        writer.WriteModule (module);
      }
    }

    [Test]
    public void OverrideMethods_SecondaryModule_MethodMarked ()
    {
      MethodDefinition secondModuleMethod = _assemblyDefinition.Modules[1].Types[1].Methods[0];
      ModuleDefinition mainModule = _assemblyDefinition.MainModule;

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _markerDefaultMarkingStrategy.AddCustomAttribute (secondModuleMethod, mainModule);

      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (3));
      Assert.That (_assemblyDefinition.MainModule.Types[2].Name, Is.EqualTo ("NonVirtualAttribute"));
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (0));

      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences[0].Name, Is.EqualTo (_assemblyDefinition.MainModule.Name));
      Assert.That (_assemblyDefinition.Modules[1].Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.Modules[1].Types[1].Methods[0].CustomAttributes[0].AttributeType.Name, Is.EqualTo ("NonVirtualAttribute"));

      TypeReference attributeType = _assemblyDefinition.MainModule.Types[2];
      Assert.That (attributeType.Scope.MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleDefinition));
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences[0].MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleReference));
      //Assert.That (attributeType.Scope.MetadataToken, Is.EqualTo (_assemblyDefinition.Modules[1].ModuleReferences[0].MetadataToken));


      IModuleDefinitionWriter writer = new AssemblyTransformer.AssemblyWriting.ModuleWriter ();
      foreach (var module in _assemblyDefinition.Modules)
      {
        Console.WriteLine ("###### " + module.Name);
        writer.WriteModule (module);
      }
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

      _markerDefaultMarkingStrategy.AddCustomAttribute (mainModuleMethod, mainModule);
      _markerDefaultMarkingStrategy.AddCustomAttribute (secondModuleMethod, mainModule);

      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (3));
      Assert.That (_assemblyDefinition.MainModule.Types[2].Name, Is.EqualTo ("NonVirtualAttribute"));
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes[0].AttributeType.Name, Is.EqualTo ("NonVirtualAttribute"));

      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences[0].Name, Is.EqualTo (_assemblyDefinition.MainModule.Name));
      Assert.That (_assemblyDefinition.Modules[1].Types[1].Methods[0].CustomAttributes.Count, Is.EqualTo (1));
      Assert.That (_assemblyDefinition.Modules[1].Types[1].Methods[0].CustomAttributes[0].AttributeType.Name, Is.EqualTo ("NonVirtualAttribute"));

      TypeReference attributeType = _assemblyDefinition.MainModule.Types[2];
      Assert.That (attributeType.Scope.MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleDefinition));
      Assert.That (_assemblyDefinition.Modules[1].ModuleReferences[0].MetadataScopeType, Is.EqualTo (MetadataScopeType.ModuleReference));
      
      //Assert.That (attributeType.Scope.MetadataToken, Is.EqualTo (_assemblyDefinition.Modules[1].ModuleReferences[0].MetadataToken));


      IModuleDefinitionWriter writer = new AssemblyTransformer.AssemblyWriting.ModuleWriter ();
      foreach (var module in _assemblyDefinition.Modules)
      {
        Console.WriteLine ("###### " + module.Name);
        writer.WriteModule (module);
      }
    }
  }
}