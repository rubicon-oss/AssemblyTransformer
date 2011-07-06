// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
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
    private ReaderParameters _readerParams;

    [SetUp]
    public void SetUp ()
    {
      _fileSystemMock = MockRepository.GenerateStrictMock<IFileSystem>();
      _factory = new DirectoryBasedAssemblyTrackerFactory (_fileSystemMock, "something");

      _assemblyDefinition1 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ();
      _assemblyDefinition2 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ();
      _assemblyDefinition3 = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ();
      _readerParams = new ReaderParameters { ReadSymbols = true };
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
      optionSet.Parse (new[] { "-y:something"});
      _factory.IncludeFiles = new List<string> () { "*.*" };
      _fileSystemMock
          .Expect (mock => mock.EnumerateFiles ("something", "*.*", SearchOption.TopDirectoryOnly))
          .Return (new[] { @"something\1.dll", @"something\2.dll", @"something\3.exe" });
      _fileSystemMock.Expect (mock => mock.FileExists (Arg.Is (@"something\1.pdb"))).Return (false);
      _fileSystemMock.Expect (mock => mock.ReadAssembly (Arg.Is (@"something\1.dll"), Arg<ReaderParameters>.Matches (r => r.ReadSymbols==false))).Return (_assemblyDefinition1);
      _fileSystemMock.Expect (mock => mock.FileExists (Arg.Is (@"something\2.pdb"))).Return (false);
      _fileSystemMock.Expect (mock => mock.ReadAssembly (Arg.Is (@"something\2.dll"), Arg<ReaderParameters>.Matches (r => r.ReadSymbols==false))).Return (_assemblyDefinition2);
      _fileSystemMock.Expect (mock => mock.FileExists (Arg.Is (@"something\3.pdb"))).Return (false);
      _fileSystemMock.Expect (mock => mock.ReadAssembly (Arg.Is (@"something\3.exe"), Arg<ReaderParameters>.Matches (r => r.ReadSymbols==false))).Return (_assemblyDefinition3);
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
      optionSet.Parse (new[] { "-d:something" });
      _factory.IncludeFiles = new List<string> () { "*.*" };
      _fileSystemMock
          .Expect (mock => mock.EnumerateFiles ("something", "*.*", SearchOption.TopDirectoryOnly))
          .Return (new[] { @"something\1.dll", @"something\2.dll" });
      _fileSystemMock.Expect (mock => mock.FileExists (Arg.Is (@"something\1.pdb"))).Return (false);
      _fileSystemMock.Expect (mock => mock.ReadAssembly (Arg.Is(@"something\1.dll"), Arg<ReaderParameters>.Matches (r => r.ReadSymbols==false))).Throw (new BadImageFormatException ("Catastrophe"));
      _fileSystemMock.Expect (mock => mock.FileExists (Arg.Is (@"something\2.pdb"))).Return (false);
      _fileSystemMock.Expect (mock => mock.ReadAssembly (Arg.Is(@"something\2.dll"), Arg<ReaderParameters>.Matches (r => r.ReadSymbols==false))).Return (_assemblyDefinition2);
      _fileSystemMock.Replay ();

      var result = _factory.CreateTracker ();

      _fileSystemMock.VerifyAllExpectations ();

      Assert.That (result, Is.TypeOf (typeof (AssemblyTracker)));
      Assert.That (((AssemblyTracker) result).GetAssemblies (), Is.EquivalentTo (new[] { _assemblyDefinition2 }));
    }
  }
}