// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Text.RegularExpressions;
using AssemblyTransformer.AssemblyMarking;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblyTracking;
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
      _marker = new AssemblyMarker (_markingAttributeStrategy);
    }

    [Test]
    public void OverrideMethods_MarksAssemblyModified ()
    {
      Regex regex = new Regex ("(.*)");
      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);

      _marker.OverrideMethods (_tracker, regex);

      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.True);
    }

    [Test]
    public void OverrideMethods_DoesNotMarkModified ()
    {
      Regex regex = new Regex ("xxxx");
      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);

      _marker.OverrideMethods (_tracker, regex);

      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);
    }

    [Test]
    public void OverrideMethods_SetsMethodVirtual ()
    {
      Regex regex = new Regex ("(.*)");
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.False);

      _marker.OverrideMethods (_tracker, regex);

      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.True);
    }

    [Test]
    public void OverrideMethods_DoesNotSetMethodVirtual ()
    {
      Regex regex = new Regex ("xxxx");
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.False);

      _marker.OverrideMethods (_tracker, regex);

      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.False);
    }

    [Test]
    public void OverrideMethods_BothMethodsMarked ()
    {
      Regex regex = new Regex ("(.*)");
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];
      MethodDefinition methodModule = _assemblyDefinition.Modules[1].Types[1].Methods[0];

      _marker.OverrideMethods (_tracker, regex);

      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodMain, _assemblyDefinition.MainModule));
      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodModule, _assemblyDefinition.MainModule));
    }

    [Test]
    public void OverrideMethods_MainModule_MethodMarked()
    {
      Regex regex = new Regex ("TestMethod");
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _marker.OverrideMethods (_tracker, regex);

      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodMain, _assemblyDefinition.MainModule));
    }

    [Test]
    public void OverrideMethods_SecondaryModule_MethodMarked ()
    {
      Regex regex = new Regex ("TestSecondMethod");
      MethodDefinition methodModule = _assemblyDefinition.Modules[1].Types[1].Methods[0];
      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _marker.OverrideMethods (_tracker, regex);

      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodModule, _assemblyDefinition.MainModule));
    }

  }

}