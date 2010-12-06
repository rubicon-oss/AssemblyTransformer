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
      _writerMock.Replay();
      var tracker = CreateTracker (_assemblyDefinition1);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations();
    }

    [Test]
    public void SignAndSave_OneModifiedAssembly_WithMultipleModules ()
    {
      var secondaryModule = ModuleDefinition.CreateModule ("Test2.netmodule", ModuleKind.NetModule);
      _assemblyDefinition1.Modules.Add (secondaryModule);

      _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      _writerMock.Expect (mock => mock.WriteModule (secondaryModule));
      _writerMock.Replay();

      var tracker = CreateTracker (_assemblyDefinition1);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations();
    }

    [Test]
    public void SignAndSave_OneModifiedAssembly_IsUnmarked ()
    {
      _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      _writerMock.Replay();
      var tracker = CreateTracker (_assemblyDefinition1);

      _assemblySigner.SignAndSave (tracker);

      Assert.That (tracker.IsModified (_assemblyDefinition1), Is.False);
    }

    [Test]
    public void SignAndSave_OneModifiedAssembly_WithReference ()
    {
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (_assemblyDefinition2.Name);
      _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      _writerMock.Replay();
      var tracker = CreateTracker (_assemblyDefinition1);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations();
    }

    [Test]
    public void SignAndSave_OneModifiedAssembly_WithReference_NotTracked ()
    {
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (new AssemblyNameReference ("Untracked", null));
      _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      _writerMock.Replay();
      var tracker = CreateTracker (_assemblyDefinition1);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations();
    }

    [Test]
    public void SignAndSave_TwoModifiedAssemblies_WithReference ()
    {
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (_assemblyDefinition2.Name);
      using (_writerMock.GetMockRepository().Ordered())
      {
        _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition2.MainModule));
        _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      }
      _writerMock.Replay();
      var tracker = CreateTracker (_assemblyDefinition1, _assemblyDefinition2);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations();
    }

    [Test]
    public void SignAndSave_TwoModifiedAssemblies_WithCircularReference ()
    {
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (_assemblyDefinition2.Name);
      _assemblyDefinition2.MainModule.AssemblyReferences.Add (_assemblyDefinition1.Name);

      using (_writerMock.GetMockRepository().Ordered())
      {
        _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition2.MainModule));
        _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule));
      }
      _writerMock.Replay();
      var tracker = CreateTracker (_assemblyDefinition1, _assemblyDefinition2);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations();
    }

    [Test]
    public void SignAndSave_TwoModifiedAssemblies_KeyChangeInReferenced ()
    {
      _assemblyDefinition2.Name.PublicKeyToken = AssemblyNameReferenceObjectMother.PublicKeyToken2;
      // we use clone because otherwise the name would be set in place and the test would always work.
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (AssemblyNameReferenceObjectMother.Clone (_assemblyDefinition2.Name));

      using (_writerMock.GetMockRepository().Ordered())
      {
        _writerMock.Expect (mock => mock.WriteModule (_assemblyDefinition2.MainModule)).WhenCalled (
            mi => ChangePublicKeyToken (_assemblyDefinition2.MainModule, AssemblyNameReferenceObjectMother.PublicKeyToken1));

        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule))
            .WhenCalled (
                mi =>
                AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition2.Name, _assemblyDefinition1.MainModule.AssemblyReferences[0]));
      }

      _writerMock.Replay();
      var tracker = CreateTracker (_assemblyDefinition1, _assemblyDefinition2);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations();
      AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition2.Name, _assemblyDefinition1.MainModule.AssemblyReferences[0]);
    }

    [Test]
    public void SignAndSave_TwoModifiedAssemblies_KeyChangeInReferenced_WithCycle ()
    {
      _assemblyDefinition1.Name.PublicKeyToken = AssemblyNameReferenceObjectMother.PublicKeyToken1;
      _assemblyDefinition2.Name.PublicKeyToken = AssemblyNameReferenceObjectMother.PublicKeyToken2;
      // we use clone because otherwise the name would be set in place and the test would always work.
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (AssemblyNameReferenceObjectMother.Clone (_assemblyDefinition2.Name));
      _assemblyDefinition2.MainModule.AssemblyReferences.Add (AssemblyNameReferenceObjectMother.Clone (_assemblyDefinition1.Name));

      using (_writerMock.GetMockRepository().Ordered())
      {
        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition2.MainModule))
            .WhenCalled (
                mi => ChangePublicKeyToken (_assemblyDefinition2.MainModule, AssemblyNameReferenceObjectMother.PublicKeyToken3))
            .Message ("First, _assemblyDefinition2 is saved and changes its public key.");

        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule))
            .WhenCalled (
                mi =>
                {
                  AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition2.Name, _assemblyDefinition1.MainModule.AssemblyReferences[0]);
                  ChangePublicKeyToken (_assemblyDefinition1.MainModule, AssemblyNameReferenceObjectMother.PublicKeyToken4);
                })
            .Message ("Second, _assemblyDefinition1 is saved (with updated reference to _assemblyDefinition2) and also changes its public key.");

        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition2.MainModule))
            .WhenCalled (
                mi =>
                AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition1.Name, _assemblyDefinition2.MainModule.AssemblyReferences[0]))
            .Message (
                "Third, _assemblyDefinition2 is saved again (with updated reference to _assemblyDefinition1), key doesn't change, recursion terminates.");
      }

      _writerMock.Replay();
      var tracker = CreateTracker (_assemblyDefinition1, _assemblyDefinition2);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations();
      AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition1.Name, _assemblyDefinition2.MainModule.AssemblyReferences[0]);
      AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition2.Name, _assemblyDefinition1.MainModule.AssemblyReferences[0]);
    }

    [Test]
    public void SignAndSave_TwoModifiedAssemblies_KeyChangeInReferenced_WithCycle_KeyChangedTwice ()
    {
      _assemblyDefinition1.Name.PublicKeyToken = AssemblyNameReferenceObjectMother.PublicKeyToken1;
      _assemblyDefinition2.Name.PublicKeyToken = AssemblyNameReferenceObjectMother.PublicKeyToken2;
      // we use clone because otherwise the name would be set in place and the test would always work.
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (AssemblyNameReferenceObjectMother.Clone (_assemblyDefinition2.Name));
      _assemblyDefinition2.MainModule.AssemblyReferences.Add (AssemblyNameReferenceObjectMother.Clone (_assemblyDefinition1.Name));

      using (_writerMock.GetMockRepository().Ordered())
      {
        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition2.MainModule))
            .WhenCalled (
                mi => ChangePublicKeyToken (_assemblyDefinition2.MainModule, AssemblyNameReferenceObjectMother.PublicKeyToken3))
            .Message ("First, _assemblyDefinition2 is saved and changes its public key.");

        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule))
            .WhenCalled (
                mi =>
                {
                  AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition2.Name, _assemblyDefinition1.MainModule.AssemblyReferences[0]);
                  ChangePublicKeyToken (_assemblyDefinition1.MainModule, AssemblyNameReferenceObjectMother.PublicKeyToken4);
                })
            .Message ("Second, _assemblyDefinition1 is saved (with updated reference to _assemblyDefinition2) and also changes its public key.");

        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition2.MainModule))
            .WhenCalled (
                mi =>
                {
                  AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition1.Name, _assemblyDefinition2.MainModule.AssemblyReferences[0]);
                  ChangePublicKeyToken (_assemblyDefinition2.MainModule, AssemblyNameReferenceObjectMother.PublicKeyToken5);
                })
            .Message ("Third, _assemblyDefinition2 is saved again (with updated reference to _assemblyDefinition1), key changes a second time.");

        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule))
            .WhenCalled (
                mi => AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition2.Name, _assemblyDefinition1.MainModule.AssemblyReferences[0]))
            .Message ("Fourth, _assemblyDefinition1 is saved again (with updated reference to _assemblyDefinition2), no key changes, recursion terminates.");
      }

      _writerMock.Replay();
      var tracker = CreateTracker (_assemblyDefinition1, _assemblyDefinition2);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations();
      
      AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition1.Name, _assemblyDefinition2.MainModule.AssemblyReferences[0]);
      AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition2.Name, _assemblyDefinition1.MainModule.AssemblyReferences[0]);
    }

    [Test]
    public void SignAndSave_TwoModifiedAssemblies_KeyChangeInReferenced_WithCycle_KeyChangedTwice_ReferncesAndDefinitionNamesAreShared ()
    {
      _assemblyDefinition1.Name.PublicKeyToken = AssemblyNameReferenceObjectMother.PublicKeyToken1;
      _assemblyDefinition2.Name.PublicKeyToken = AssemblyNameReferenceObjectMother.PublicKeyToken2;
      _assemblyDefinition1.MainModule.AssemblyReferences.Add (_assemblyDefinition2.Name);
      _assemblyDefinition2.MainModule.AssemblyReferences.Add (_assemblyDefinition1.Name);

      using (_writerMock.GetMockRepository ().Ordered ())
      {
        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition2.MainModule))
            .WhenCalled (
                mi => ChangePublicKeyToken (_assemblyDefinition2.MainModule, AssemblyNameReferenceObjectMother.PublicKeyToken3))
            .Message ("First, _assemblyDefinition2 is saved and changes its public key.");

        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule))
            .WhenCalled (
                mi =>
                {
                  AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition2.Name, _assemblyDefinition1.MainModule.AssemblyReferences[0]);
                  ChangePublicKeyToken (_assemblyDefinition1.MainModule, AssemblyNameReferenceObjectMother.PublicKeyToken4);
                })
            .Message ("Second, _assemblyDefinition1 is saved (with updated reference to _assemblyDefinition2) and also changes its public key.");

        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition2.MainModule))
            .WhenCalled (
                mi =>
                {
                  AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition1.Name, _assemblyDefinition2.MainModule.AssemblyReferences[0]);
                  ChangePublicKeyToken (_assemblyDefinition2.MainModule, AssemblyNameReferenceObjectMother.PublicKeyToken5);
                })
            .Message ("Third, _assemblyDefinition2 is saved again (with updated reference to _assemblyDefinition1), key changes a second time.");

        _writerMock
            .Expect (mock => mock.WriteModule (_assemblyDefinition1.MainModule))
            .WhenCalled (
                mi => AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition2.Name, _assemblyDefinition1.MainModule.AssemblyReferences[0]))
            .Message ("Fourth, _assemblyDefinition1 is saved again (with updated reference to _assemblyDefinition2), no key changes, recursion terminates.");
      }

      _writerMock.Replay ();
      var tracker = CreateTracker (_assemblyDefinition1, _assemblyDefinition2);

      _assemblySigner.SignAndSave (tracker);

      _writerMock.VerifyAllExpectations ();

      AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition1.Name, _assemblyDefinition2.MainModule.AssemblyReferences[0]);
      AssemblyNameReferenceChecker.CheckNameReferences (_assemblyDefinition2.Name, _assemblyDefinition1.MainModule.AssemblyReferences[0]);
    }

    private void ChangePublicKeyToken (ModuleDefinition writtenModule, byte[] newToken)
    {
      writtenModule.Assembly.Name.PublicKeyToken = newToken;
      Assert.That (writtenModule.Assembly.Name.PublicKeyToken, Is.EqualTo (newToken));
    }

    private IAssemblyTracker CreateTracker (params AssemblyDefinition[] modifiedAssemblies)
    {
      // Should AssemblyTracker get external dependencies (or become otherwise more complex), consider using a stub instead of the real tracker.
      var tracker = new AssemblyTracker (new[] { _assemblyDefinition1, _assemblyDefinition2 });

      foreach (var modifiedAssembly in modifiedAssemblies)
        tracker.MarkModified (modifiedAssembly);

      return tracker;
    }
  }
}