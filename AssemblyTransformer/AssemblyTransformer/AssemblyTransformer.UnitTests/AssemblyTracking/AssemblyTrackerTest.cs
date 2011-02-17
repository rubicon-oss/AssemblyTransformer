// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;
using NUnit.Framework;

namespace AssemblyTransformer.UnitTests.AssemblyTracking
{
  [TestFixture]
  public class AssemblyTrackerTest
  {
    private AssemblyDefinition _assemblyDefinition1;
    private AssemblyDefinition _assemblyDefinition2;

    [SetUp]
    public void SetUp ()
    {
      _assemblyDefinition1 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1");
      _assemblyDefinition2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase2");
    }

    [Test]
    public void Initialization ()
    {
      AssemblyTracker tracker = new AssemblyTracker(new [] {  _assemblyDefinition1, _assemblyDefinition2});
      Assert.That (tracker.GetAssemblies(), Is.EquivalentTo (new[] { _assemblyDefinition1, _assemblyDefinition2 }));
    }

    [Test]
    public void Initialization_ReferenceNotTracked ()
    {
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (_assemblyDefinition2.Name);
      AssemblyTracker tracker = new AssemblyTracker (new[] { _assemblyDefinition1 });
      
      Assert.That (tracker.GetAssemblies (), Is.EquivalentTo (new[] { _assemblyDefinition1 }));
    }

    [Test]
    public void GetAssemblyByReference ()
    {
      var name = AssemblyNameReferenceObjectMother.CreateAssemblyNameReference("TestCase1");

      AssemblyDefinition assembly = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1");
      AssemblyDefinition assembly2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase2");
      AssemblyTracker tracker = new AssemblyTracker (new[] { assembly, assembly2 });

      var result = tracker.GetAssemblyByReference (name);

      Assert.That (result, Is.EqualTo (new [] {assembly}));

    }

    [Test]
    public void GetAssemblyByReference_MatchesFullReference ()
    {
      var name = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithCulture ("TestCase1", "de");

      AssemblyDefinition assembly = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1", "de");
      AssemblyDefinition assembly2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1");
      AssemblyTracker tracker = new AssemblyTracker (new[] { assembly, assembly2 });

      var result = tracker.GetAssemblyByReference (name);

      Assert.That (result, Is.EqualTo (new [] {assembly}));
    }

    [Test]
    public void GetAssemblyByReference_NoMatchFound ()
    {
      var name = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithCulture ("TestCase1", "de");

      AssemblyDefinition assembly = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1");
      AssemblyDefinition assembly2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase2");
      AssemblyTracker tracker = new AssemblyTracker (new[] { assembly, assembly2 });

      var result = tracker.GetAssemblyByReference (name);

      Assert.That (result, Is.Empty);
    }

    [Test]
    public void IsModified_NotTracked ()
    {
      AssemblyTracker tracker = new AssemblyTracker (new[] { _assemblyDefinition1 });

      Assert.Throws<ArgumentException> (() => tracker.IsModified (_assemblyDefinition2));
    }

    [Test]
    public void GetModifiedAssemblies ()
    {
      AssemblyTracker tracker = new AssemblyTracker (new[] { _assemblyDefinition1, _assemblyDefinition2 });
      tracker.MarkModified (_assemblyDefinition1);

      var result = tracker.GetModifiedAssemblies();
      Assert.That (result, Is.EquivalentTo (new[] { _assemblyDefinition1 }));
    }

    [Test]
    public void MarkModified ()
    {
      AssemblyTracker tracker = new AssemblyTracker (new[] { _assemblyDefinition1, _assemblyDefinition2 });

      tracker.MarkModified (_assemblyDefinition1);

      Assert.That (tracker.IsModified (_assemblyDefinition1), Is.True);
      Assert.That (tracker.IsModified (_assemblyDefinition2), Is.False);
    }

    [Test]
    public void MarkModified_NotTracked()
    {
      AssemblyTracker tracker = new AssemblyTracker (new[] { _assemblyDefinition1 });

      Assert.Throws<ArgumentException> (() => tracker.MarkModified (_assemblyDefinition2));
    }

    [Test]
    public void MarkUnmodified ()
    {
      AssemblyTracker tracker = new AssemblyTracker (new[] { _assemblyDefinition1 });
      tracker.MarkModified (_assemblyDefinition1);
      Assert.That (tracker.IsModified (_assemblyDefinition1), Is.True);

      tracker.MarkUnmodified (_assemblyDefinition1);

      Assert.That (tracker.IsModified (_assemblyDefinition1), Is.False);
    }

    [Test]
    public void MarkUnodified_NotTracked ()
    {
      AssemblyTracker tracker = new AssemblyTracker (new[] { _assemblyDefinition1 });

      Assert.Throws<ArgumentException> (() => tracker.MarkUnmodified (_assemblyDefinition2));
    }

    [Test]
    public void GetReverseReferences ()
    {
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (_assemblyDefinition2.Name);
      AssemblyTracker tracker = new AssemblyTracker (new[] { _assemblyDefinition1, _assemblyDefinition2 });

      var reverseReferences = tracker.GetReverseReferences (_assemblyDefinition2);

      Assert.That (reverseReferences, Is.EquivalentTo (new [] {_assemblyDefinition1}));
    }

    [Test]
    public void GetReverseReferences_NotTracked ()
    {
      AssemblyTracker tracker = new AssemblyTracker (new AssemblyDefinition[0]);

      Assert.Throws<ArgumentException> (() => tracker.GetReverseReferences (_assemblyDefinition1));
    }

    [Test]
    public void GetReverseReferences_MultiModuleAssembly ()
    {
      ModuleDefinition module = ModuleDefinition.CreateModule ("Module1", ModuleKind.NetModule);
      module.AssemblyReferences.Add (_assemblyDefinition2.Name);
      _assemblyDefinition1.Modules.Add (module);
      
      AssemblyTracker tracker = new AssemblyTracker (new[] { _assemblyDefinition1, _assemblyDefinition2 });

      var reverseReferences = tracker.GetReverseReferences (_assemblyDefinition2);

      Assert.That (reverseReferences, Is.EquivalentTo (new[] { _assemblyDefinition1 }));
    }
  }
}