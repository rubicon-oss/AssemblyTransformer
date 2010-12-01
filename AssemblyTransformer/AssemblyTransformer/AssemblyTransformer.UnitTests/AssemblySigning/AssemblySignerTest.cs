// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;

namespace AssemblyTransformer.UnitTests.AssemblySigning
{
  [TestFixture]
  public class AssemblySignerTest
  {
    private AssemblyDefinition _assemblyDefinition1;
    private AssemblyDefinition _assemblyDefinition2;
    private IModuleDefinitionWriter _writerMock;
    private AssemblySigner _assemblySigner;

    [SetUp]
    public void SetUp ()
    {
      _assemblyDefinition1 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase1");
      _assemblyDefinition2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestCase2");

      _writerMock = MockRepository.GenerateStrictMock<IModuleDefinitionWriter>();
      _assemblySigner = new AssemblySigner (_writerMock);
    }

    [Test]
    public void SignAndSave_NoModifiedAssemblies ()
    {
      _writerMock.Replay();
      var tracker = CreateTracker();

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations();
    }

    [Test]
    public void SignAndSave_OneModifiedAssembly ()
    {
      _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      _writerMock.Replay ();
      var tracker = CreateTracker (_assemblyDefinition1);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations ();
    }

    [Test]
    public void SignAndSave_OneModifiedAssembly_WithMultipleModules ()
    {
      var secondaryModule = ModuleDefinition.CreateModule ("Test2.netmodule", ModuleKind.NetModule);
      _assemblyDefinition1.Modules.Add (secondaryModule);

      _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      _writerMock.Expect (mock => mock.WriteModule (secondaryModule));
      _writerMock.Replay ();
      
      var tracker = CreateTracker (_assemblyDefinition1);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations ();
    }

    [Test]
    public void SignAndSave_OneModifiedAssembly_IsUnmarked ()
    {
      _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      _writerMock.Replay ();
      var tracker = CreateTracker (_assemblyDefinition1);

      _assemblySigner.SignAndSave (tracker);

      Assert.That (tracker.IsModified (_assemblyDefinition1), Is.False);
    }

    [Test]
    public void SignAndSave_OneModifiedAssembly_WithReference ()
    {
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (_assemblyDefinition2.Name);
      _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      _writerMock.Replay ();
      var tracker = CreateTracker (_assemblyDefinition1);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations ();
    }

    [Test]
    public void SignAndSave_OneModifiedAssembly_WithReference_NotTracked ()
    {
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (new AssemblyNameReference ("Untracked", null));
      _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      _writerMock.Replay ();
      var tracker = CreateTracker (_assemblyDefinition1);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations ();
    }

    [Test]
    public void SignAndSave_TwoModifiedAssemblies_WithReference ()
    {
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (_assemblyDefinition2.Name);
      using (_writerMock.GetMockRepository ().Ordered ())
      {
        _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition2.MainModule));
        _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      }
      _writerMock.Replay ();
      var tracker = CreateTracker (_assemblyDefinition1, _assemblyDefinition2);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations ();
    }

    [Test]
    public void SignAndSave_TwoModifiedAssemblies_WithCircularReference ()
    {
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (_assemblyDefinition2.Name);
      _assemblyDefinition2.MainModule.AssemblyReferences.Add (_assemblyDefinition1.Name);

      using (_writerMock.GetMockRepository ().Ordered ())
      {
        _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition2.MainModule));
        _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      }
      _writerMock.Replay ();
      var tracker = CreateTracker (_assemblyDefinition1, _assemblyDefinition2);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations ();
    }

    [Test]
    public void SignAndSave_TwoModifiedAssemblies_KeyChangeInReferenced ()
    {
      Assert.That (_assemblyDefinition2.Name.PublicKeyToken, Is.Null);

      _assemblyDefinition1.MainModule.AssemblyReferences.Add (AssemblyNameReferenceObjectMother.Clone (_assemblyDefinition2.Name));
      Assert.That (_assemblyDefinition1.MainModule.AssemblyReferences[0].PublicKeyToken, Is.Null);

      using (_writerMock.GetMockRepository ().Ordered ())
      {
        _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition2.MainModule)).WhenCalled (mi =>
        {
          var writtenModule = (ModuleDefinition) mi.Arguments[0];
          writtenModule.Assembly.Name.PublicKeyToken = AssemblyNameReferenceObjectMother.PublicKeyToken1;
          Assert.That (_assemblyDefinition2.Name.PublicKeyToken, Is.EqualTo (AssemblyNameReferenceObjectMother.PublicKeyToken1));
        });

        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule))
            .WhenCalled (mi => 
                Assert.That (
                    _assemblyDefinition1.MainModule.AssemblyReferences[0].PublicKeyToken,
                    Is.EqualTo (AssemblyNameReferenceObjectMother.PublicKeyToken1)));
      }

      _writerMock.Replay ();
      var tracker = CreateTracker (_assemblyDefinition1, _assemblyDefinition2);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations ();
    }

    private IAssemblyTracker CreateTracker (params AssemblyDefinition[] modifiedAssemblies)
    {
      // Use a real AssemblyTracker instead of a stub or mock because the tests rely on the interplay of the different MarkModified/IsModified/etc.
      // methods. This is really hard to simulate using Rhino Mocks. Should AssemblyTracker get external dependencies (or become otherwise more 
      // complex), consider implementing a TestTracker stub class just for these tests here.
      var tracker = new AssemblyTracker (new[] { _assemblyDefinition1, _assemblyDefinition2 });

      foreach (var modifiedAssembly in modifiedAssemblies)
        tracker.MarkModified (modifiedAssembly);

      return tracker;
    }

  }
}