// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using Mono.Cecil;
using NUnit.Framework;

namespace AssemblyTransformer.UnitTests
{
  [TestFixture]
  public class TrackedAssemblyTest
  {
    private AssemblyDefinition _assemblyDefinition;

    [SetUp]
    public void SetUp ()
    {
      _assemblyDefinition = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ();
    }

    [Test]
    public void Initialization ()
    {
      TrackedAssembly assembly = new TrackedAssembly(_assemblyDefinition);

      Assert.That (assembly.AssemblyDefinition, Is.SameAs (_assemblyDefinition));
      Assert.That (assembly.IsModified, Is.False);
      Assert.That (assembly.ReverseReferences, Is.Empty);
    }

    [Test]
    public void AddReverseReference ()
    {
      TrackedAssembly assembly = new TrackedAssembly (_assemblyDefinition);
      TrackedAssembly assembly2 = new TrackedAssembly (_assemblyDefinition);

      assembly.AddReverseReference (assembly2);

      Assert.That (assembly.ReverseReferences, Has.Member (assembly2));
    }

    [Test]
    public void MarkModified ()
    {
      TrackedAssembly assembly = new TrackedAssembly (_assemblyDefinition);
      Assert.That (assembly.IsModified, Is.False);

      assembly.MarkModified();

      Assert.That (assembly.IsModified, Is.True);
    }

    [Test]
    public void MarkModified_AlsoMarksReverseReference ()
    {
      TrackedAssembly assembly = new TrackedAssembly (_assemblyDefinition);
      TrackedAssembly assembly2 = new TrackedAssembly (_assemblyDefinition);
      assembly.AddReverseReference (assembly2);
      Assert.That (assembly.IsModified, Is.False);
      Assert.That (assembly2.IsModified, Is.False);
     
      assembly.MarkModified ();

      Assert.That (assembly.IsModified, Is.True);
      Assert.That (assembly2.IsModified, Is.True);
    }

    [Test]
    public void MarkModified_AlsoMarksReverseReference_Recursive ()
    {
      TrackedAssembly assembly = new TrackedAssembly (_assemblyDefinition);
      TrackedAssembly assembly2 = new TrackedAssembly (_assemblyDefinition);
      TrackedAssembly assembly3 = new TrackedAssembly (_assemblyDefinition);
      assembly.AddReverseReference (assembly2);
      assembly2.AddReverseReference (assembly3);
      Assert.That (assembly.IsModified, Is.False);
      Assert.That (assembly2.IsModified, Is.False);
      Assert.That (assembly3.IsModified, Is.False);

      assembly.MarkModified ();

      Assert.That (assembly.IsModified, Is.True);
      Assert.That (assembly2.IsModified, Is.True);
      Assert.That (assembly3.IsModified, Is.True);
    }

    [Test]
    public void MarkModified_CanHandleCyclicReferences ()
    {
      TrackedAssembly assembly = new TrackedAssembly (_assemblyDefinition);
      TrackedAssembly assembly2 = new TrackedAssembly (_assemblyDefinition);
      TrackedAssembly assembly3 = new TrackedAssembly (_assemblyDefinition);
      assembly.AddReverseReference (assembly2);
      assembly2.AddReverseReference (assembly3);
      assembly3.AddReverseReference (assembly);
      Assert.That (assembly.IsModified, Is.False);
      Assert.That (assembly2.IsModified, Is.False);
      Assert.That (assembly3.IsModified, Is.False);

      assembly.MarkModified ();

      Assert.That (assembly.IsModified, Is.True);
      Assert.That (assembly2.IsModified, Is.True);
      Assert.That (assembly3.IsModified, Is.True);
    }

    [Test]
    public void MarkUnmodified ()
    {
      TrackedAssembly assembly = new TrackedAssembly (_assemblyDefinition);
      assembly.MarkModified();
      Assert.That (assembly.IsModified, Is.True);

      assembly.MarkUnmodified ();

      Assert.That (assembly.IsModified, Is.False);
    }


  }
}