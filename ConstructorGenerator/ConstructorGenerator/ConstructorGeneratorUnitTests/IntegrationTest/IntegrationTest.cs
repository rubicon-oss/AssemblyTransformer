// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblySigning.AssemblyWriting;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;
using AssemblyTransformer.FileSystem;
using AssemblyTransformer.TypeDefinitionCaching;
using ConstructorGenerator;
using ConstructorGenerator.CodeGenerator;
using ConstructorGenerator.MixinChecker;
using ConstructorGenerator.ReferenceGenerator;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Utilities;
using Rhino.Mocks;

namespace ConstructorGeneratorUnitTests
{
  [TestFixture]
  public class IntegrationTest
  {

    private string AssemblyPath;
    private string TempPath;
    private PEVerifier _verifier;

    private IAssemblyTracker _tracker;
    private IAssemblyTransformation _transformator;
    private IAssemblySigner _signer;
    private IFileSystem _fileSystem;
    private IMixinChecker _checker;


    [SetUp]
    public void SetUp ()
    {
      AssemblyPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "IntegrationTestFiles");
      TempPath = Path.Combine (AssemblyPath, "temp");
      
      if (Directory.Exists (TempPath))
        Directory.Delete (TempPath, true);
      Directory.CreateDirectory (TempPath);

      Thread.Sleep (500);

      var assemblyFileNames = new[] { "DummyTarget.dll" };
      foreach (var assemblyFileName in assemblyFileNames)
        File.Copy (Path.Combine (AssemblyPath, assemblyFileName), GetTempPath (assemblyFileName));

      _verifier = PEVerifier.CreateDefault ();

      AssemblyDefinition[] assemblies = assemblyFileNames.Select (fileName => AssemblyDefinition.ReadAssembly (GetTempPath (fileName))).ToArray();
      _tracker = new AssemblyTracker (assemblies, new TypeDefinitionCache());

      _checker = MockRepository.GenerateStub<IMixinChecker> ();
      _checker.Stub (x => x.CanBeMixed (null)).Return (true).IgnoreArguments ();

      _transformator = new ConstructorGenerator.ConstructorGenerator (_checker,
        new ILCodeGenerator (
          new MethodReferenceGenerator ("Remotion.Interfaces, Version=1.13.73.1026, Culture=neutral, PublicKeyToken=fee00910d6e5f53b", 
                                        "Remotion.Mixins.ObjectFactory",
                                        "Remotion.Reflection.ParamList"), 
           _checker)
          );

      _signer = new AssemblySigner (new ModuleDefinitionWriter (new FileSystem (), null, new List<StrongNameKeyPair> ()));

      _fileSystem = new FileSystem();
    }

    private string GetTempPath (string fileName)
    {
      return Path.Combine (TempPath, fileName);
    }
    
    //[TearDown]
    //public void TearDown ()
    //{
    //  File.Copy (AssemblyPath + @"\..\Remotion.Interfaces.dll", TempPath + @"\Remotion.Interfaces.dll");
    //  _verifier.VerifyPEFile (TempPath + @"\DummyTarget.dll");

    //  foreach (var file in Directory.EnumerateFiles (TempPath, "*", SearchOption.AllDirectories))
    //    File.Delete (file);
    //}

    [Test]
    public void GeneratesPEVerifyable ()
    {
      _transformator.Transform (_tracker);
      _signer.SignAndSave (_tracker);

      File.Copy (AssemblyPath + @"\Remotion.Interfaces.dll", TempPath + @"\Remotion.Interfaces.dll");
      _verifier.VerifyPEFile (TempPath + @"\DummyTarget.dll");

    }

    [Test]
    public void CreatesNewObjectMethods ()
    {
      _transformator.Transform (_tracker);
      _signer.SignAndSave (_tracker);
      foreach (var def in _tracker.GetModifiedAssemblies())
      {
        foreach (var typ in def.MainModule.Types)
        {
          Assert.That (typ.Methods.Select (m => m.Name == "NewObject").Count(), Is.EqualTo (3));
        }
      }
      foreach (var file in Directory.EnumerateFiles (TempPath, "*", SearchOption.AllDirectories))
        File.Delete (file);
    }

    [Test]
    public void ReplacesNewStatement ()
    {
      _transformator.Transform (_tracker);
      _signer.SignAndSave (_tracker);
      foreach (var def in _tracker.GetModifiedAssemblies ())
      {
        foreach (var typ in def.MainModule.Types)
        {
          foreach (var meth in typ.Methods)
          {
            foreach (var instruction in meth.Body.Instructions)
            {
              Assert.That (instruction.OpCode != OpCodes.Newobj);
            }
          }
        }
      }
      foreach (var file in Directory.EnumerateFiles (TempPath, "*", SearchOption.AllDirectories))
        File.Delete (file);
    }
  }
}