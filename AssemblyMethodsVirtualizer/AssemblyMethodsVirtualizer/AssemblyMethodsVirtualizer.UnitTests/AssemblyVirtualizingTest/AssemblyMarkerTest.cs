// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Text.RegularExpressions;
using AssemblyMethodsVirtualizer.MarkingStrategies;
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;

namespace AssemblyMethodsVirtualizer.UnitTests.AssemblyVirtualizingTest
{
  [TestFixture]
  public class AssemblyMarkerTest
  {
    private IAssemblyTracker _tracker;
    private AssemblyDefinition _assemblyDefinition;

    private AssemblyMethodsVirtualizer _methodsVirtualizer;

    private IMarkingAttributeStrategy _markingAttributeStrategy;

    [SetUp]
    public void SetUp ()
    {
      _assemblyDefinition = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition();
      _tracker = new AssemblyTracker (new [] { _assemblyDefinition });
      _markingAttributeStrategy = MockRepository.GenerateStub<IMarkingAttributeStrategy> ();
      _methodsVirtualizer = new AssemblyMethodsVirtualizer (_markingAttributeStrategy, new Regex ("(.*)"));
    }

    [Test]
    public void OverrideMethods_MarksAssemblyWithMatchingMethodsModified ()
    {
      Regex regex = new Regex ("(.*)");
      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);

      _methodsVirtualizer.Transform (_tracker);

      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.True);
    }

    [Test]
    public void OverrideMethods_DoesNotMarkAssemblyWithougMatchingMethodsModified ()
    {
      var _methodsVirtualizerNoMatch = new AssemblyMethodsVirtualizer (_markingAttributeStrategy, new Regex ("xxxx"));
      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);

      _methodsVirtualizerNoMatch.Transform (_tracker);

      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);
    }

    [Test]
    public void OverrideMethods_SetsMatchingMethodVirtual ()
    {
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.False);

      _methodsVirtualizer.Transform (_tracker);

      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.True);
    }

    [Test]
    public void OverrideMethods_DoesNotSetNonMatchingMethodVirtual ()
    {
      _methodsVirtualizer = new AssemblyMethodsVirtualizer (_markingAttributeStrategy, new Regex ("xxxx"));
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.False);

      _methodsVirtualizer.Transform (_tracker);

      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.False);
    }

    [Test]
    public void OverrideMethods_BothMethodsMarked ()
    {
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];
      MethodDefinition methodModule = _assemblyDefinition.Modules[1].Types[1].Methods[0];

      _methodsVirtualizer.Transform (_tracker);

      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodMain, _assemblyDefinition));
      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodModule, _assemblyDefinition));
    }

    [Test]
    public void OverrideMethods_MainModule_MethodMarked()
    {
      _methodsVirtualizer = new AssemblyMethodsVirtualizer (_markingAttributeStrategy, new Regex ("TestMethod"));
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _methodsVirtualizer.Transform (_tracker);

      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodMain, _assemblyDefinition));
    }

    [Test]
    public void OverrideMethods_SecondaryModule_MethodMarked ()
    {
      _methodsVirtualizer = new AssemblyMethodsVirtualizer (_markingAttributeStrategy, new Regex ("TestSecondMethod"));
      MethodDefinition methodModule = _assemblyDefinition.Modules[1].Types[1].Methods[0];
      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _methodsVirtualizer.Transform (_tracker);

      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodModule, _assemblyDefinition));
    }

  }

}