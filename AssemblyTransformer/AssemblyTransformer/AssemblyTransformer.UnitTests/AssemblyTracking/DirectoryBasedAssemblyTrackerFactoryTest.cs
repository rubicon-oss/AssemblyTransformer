// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.IO;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;

namespace AssemblyTransformer.UnitTests.AssemblyTracking
{
  [TestFixture]
  public class DirectoryBasedAssemblyTrackerFactoryTest
  {
    private IFileSystem _fileSystemMock;
    private DirectoryBasedAssemblyTrackerFactory _factory;

    private AssemblyDefinition _assemblyDefinition1;
    private AssemblyDefinition _assemblyDefinition2;
    private AssemblyDefinition _assemblyDefinition3;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem>();
      _factory = new DirectoryBasedAssemblyTrackerFactory (_fileSystemMock);

      _assemblyDefinition1 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ();
      _assemblyDefinition2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ();
      _assemblyDefinition3 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void CreateTracker_NotInitialized ()
    {
      _factory.CreateTracker();
    }

    [Test]
    public void CreateTracker ()
    {
      var optionSet = new OptionSet();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "-d:something", "-w=*.*" });

      _fileSystemMock
          .Expect (mock => mock.EnumerateFiles ("something", "*.*", SearchOption.AllDirectories))
          .Return (new[] { @"something\1.dll", @"something\2.dll", @"something\3.exe" });
      _fileSystemMock.Expect (mock => mock.ReadAssembly (@"something\1.dll")).Return (_assemblyDefinition1);
      _fileSystemMock.Expect (mock => mock.ReadAssembly (@"something\2.dll")).Return (_assemblyDefinition2);
      _fileSystemMock.Expect (mock => mock.ReadAssembly (@"something\3.exe")).Return (_assemblyDefinition3);
      _fileSystemMock.Replay();
      
      var result = _factory.CreateTracker();

      _fileSystemMock.VerifyAllExpectations();

      Assert.That (result, Is.TypeOf (typeof (AssemblyTracker)));
      Assert.That (
          ((AssemblyTracker) result).GetAssemblies(), Is.EquivalentTo (new[] { _assemblyDefinition1, _assemblyDefinition2, _assemblyDefinition3 }));
    }

    [Test]
    public void CreateTracker_IgnoresExceptions ()
    {
      var optionSet = new OptionSet ();
      _factory.AddOptions (optionSet);
      optionSet.Parse (new[] { "-d:something", "-w=*.*" });

      _fileSystemMock
          .Expect (mock => mock.EnumerateFiles ("something", "*.*", SearchOption.AllDirectories))
          .Return (new[] { @"something\1.dll", @"something\2.dll" });
      _fileSystemMock.Expect (mock => mock.ReadAssembly (@"something\1.dll")).Throw (new BadImageFormatException ("Catastrophe"));
      _fileSystemMock.Expect (mock => mock.ReadAssembly (@"something\2.dll")).Return (_assemblyDefinition2);
      _fileSystemMock.Replay ();

      var result = _factory.CreateTracker ();

      _fileSystemMock.VerifyAllExpectations ();

      Assert.That (result, Is.TypeOf (typeof (AssemblyTracker)));
      Assert.That (((AssemblyTracker) result).GetAssemblies (), Is.EquivalentTo (new[] { _assemblyDefinition2 }));
    }
  }
}