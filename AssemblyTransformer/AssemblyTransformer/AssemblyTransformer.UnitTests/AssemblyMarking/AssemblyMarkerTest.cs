// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Text.RegularExpressions;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations.AssemblyMarking;
using AssemblyTransformer.AssemblyTransformations.AssemblyMarking.MarkingStrategies;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;

namespace AssemblyTransformer.UnitTests.AssemblyMarking
{
  [TestFixture]
  public class AssemblyMarkerTest
  {
    private IAssemblyTracker _tracker;
    private AssemblyDefinition _assemblyDefinition;

    private AssemblyMarker _marker;

    private IMarkingAttributeStrategy _markingAttributeStrategy;

    [SetUp]
    public void SetUp ()
    {
      _assemblyDefinition = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition();
      _tracker = new AssemblyTracker (new [] { _assemblyDefinition });
      _markingAttributeStrategy = MockRepository.GenerateStub<IMarkingAttributeStrategy> ();
      _marker = new AssemblyMarker (_markingAttributeStrategy, new Regex ("(.*)"));
    }

    [Test]
    public void OverrideMethods_MarksAssemblyWithMatchingMethodsModified ()
    {
      Regex regex = new Regex ("(.*)");
      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);

      _marker.Transform (_tracker);

      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.True);
    }

    [Test]
    public void OverrideMethods_DoesNotMarkAssemblyWithougMatchingMethodsModified ()
    {
      // TODO Review FS: Use a second marker field for the "xxxx" marker for the non-matching tests (eg., _markerWithNonMatchingRegex) instead of changing _marker. When initializing a field in the SetUp method, avoid changing it to a different reference later on.
      _marker = new AssemblyMarker (_markingAttributeStrategy, new Regex ("xxxx"));
      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);

      _marker.Transform (_tracker);

      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);
    }

    [Test]
    public void OverrideMethods_SetsMatchingMethodVirtual ()
    {
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.False);

      _marker.Transform (_tracker);

      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.True);
    }

    [Test]
    public void OverrideMethods_DoesNotSetNonMatchingMethodVirtual ()
    {
      _marker = new AssemblyMarker (_markingAttributeStrategy, new Regex ("xxxx"));
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.False);

      _marker.Transform (_tracker);

      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.False);
    }

    [Test]
    public void OverrideMethods_BothMethodsMarked ()
    {
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];
      MethodDefinition methodModule = _assemblyDefinition.Modules[1].Types[1].Methods[0];

      _marker.Transform (_tracker);

      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodMain, _assemblyDefinition));
      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodModule, _assemblyDefinition));
    }

    [Test]
    public void OverrideMethods_MainModule_MethodMarked()
    {
      _marker = new AssemblyMarker (_markingAttributeStrategy, new Regex ("TestMethod"));
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _marker.Transform (_tracker);

      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodMain, _assemblyDefinition));
    }

    [Test]
    public void OverrideMethods_SecondaryModule_MethodMarked ()
    {
      _marker = new AssemblyMarker (_markingAttributeStrategy, new Regex ("TestSecondMethod"));
      MethodDefinition methodModule = _assemblyDefinition.Modules[1].Types[1].Methods[0];
      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _marker.Transform (_tracker);

      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodModule, _assemblyDefinition));
    }

  }

}