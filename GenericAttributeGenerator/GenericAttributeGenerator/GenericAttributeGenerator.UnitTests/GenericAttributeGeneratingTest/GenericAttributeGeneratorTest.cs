// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;
using NUnit.Framework;

namespace GenericAttributeGenerator.UnitTests.GenericAttributeGeneratingTest
{
  [TestFixture]
  public class GenericAttributeGeneratorTest
  {
    private IAssemblyTracker _tracker;
    private AssemblyDefinition _assemblyDefinition;
    private GenericAttributeGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _assemblyDefinition = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition ();
      _tracker = new AssemblyTracker (new[] { _assemblyDefinition });
      _generator = new GenericAttributeGenerator (typeof(GenericAttributeMarkerAttribute));
    }

    [Test]
    public void Transform_NoModification ()
    {
      Assert.That (_tracker.IsModified(_assemblyDefinition), Is.False);

      _generator.Transform (_tracker);

      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);
    }

    [Test]
    public void Transform_ModifiesAssembly ()
    {
      var attCtor = _assemblyDefinition.MainModule.Import (typeof (GenericAttributeMarkerAttribute).GetConstructor (Type.EmptyTypes));
      _assemblyDefinition.MainModule.Types[1].Methods[0].CustomAttributes.Add (new CustomAttribute (attCtor));
      Console.WriteLine (_assemblyDefinition.MainModule.Types[1].Methods[0].Name);
      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);

      _generator.Transform (_tracker);

      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.True);
    }
  }
}