// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using Mono.Cecil;
using NUnit.Framework;

namespace AssemblyTransformer.UnitTests
{
  [TestFixture]
  public class AssemblyTrackerTest
  {

    [Test]
    public void Initialization ()
    {
      AssemblyDefinition assembly = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1");
      AssemblyDefinition assembly2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase2");
      
      AssemblyTracker tracker = new AssemblyTracker(new [] {  assembly, assembly2});

      Assert.That (tracker.GetAssemblies(), Is.EquivalentTo (new[] { assembly, assembly2 }));
    }

    [Test]
    public void GetAssemblyByReference ()
    {
      var name = AssemblyNameReferenceObjectMother.CreateAssemblyNameReference("TestCase1");

      AssemblyDefinition assembly = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1");
      AssemblyDefinition assembly2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase2");
      AssemblyTracker tracker = new AssemblyTracker (new[] { assembly, assembly2 });

      var result = tracker.GetAssemblyByReference (name);

      Assert.That (result, Is.SameAs (assembly));

    }

    [Test]
    public void GetAssemblyByReference_MatchesFullReference ()
    {
      var name = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithCulture ("TestCase1", "de");

      AssemblyDefinition assembly = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1", "de");
      AssemblyDefinition assembly2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1", "en");
      AssemblyTracker tracker = new AssemblyTracker (new[] { assembly, assembly2 });

      var result = tracker.GetAssemblyByReference (name);

      Assert.That (result, Is.SameAs (assembly));
    }

    [Test]
    public void GetAssemblyByReference_NoMatchFound ()
    {
      var name = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithCulture ("TestCase1", "de");

      AssemblyDefinition assembly = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1");
      AssemblyDefinition assembly2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1", "en");
      AssemblyTracker tracker = new AssemblyTracker (new[] { assembly, assembly2 });

      var result = tracker.GetAssemblyByReference (name);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void MarkModified ()
    {
      AssemblyDefinition assembly = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1");
      AssemblyDefinition assembly2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1", "en");
      AssemblyTracker tracker = new AssemblyTracker (new[] { assembly, assembly2 });

      tracker.MarkModified (assembly);

      Assert.That (tracker.IsModified (assembly), Is.True);
      Assert.That (tracker.IsModified (assembly2), Is.False);
    }

    [Test]
    public void MarkModified_NotFound()
    {
      AssemblyDefinition assembly = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1");
      AssemblyDefinition assembly2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1", "en");
      AssemblyTracker tracker = new AssemblyTracker (new[] { assembly });

      Assert.Throws<ArgumentException> (() => tracker.MarkModified (assembly2));
    }

    [Test]
    public void IsModified_NotFound ()
    {
      AssemblyDefinition assembly = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1");
      AssemblyDefinition assembly2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1", "en");
      AssemblyTracker tracker = new AssemblyTracker (new[] { assembly });

      Assert.Throws<ArgumentException> (() => tracker.IsModified (assembly2));
    }
  }
}