// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblySigning.AssemblyWriting;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;
using Mono.Cecil;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using System.Collections.Generic;
using AssemblyMethodsVirtualizer.MarkingStrategies;
using AssemblyMethodsVirtualizer;

namespace AssemblyTransformer.UnitTests.IntegrationTests
{
  [TestFixture]
  public class IntegrationTest
  {
    private static readonly string AssemblyPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\prereq\testing\integration");
    private static readonly string TempPath = AssemblyPath + @"\temp";
    private PEVerifier _verifier;

    private IAssemblyTracker _tracker;
    private IAssemblyTransformation _transformator;
    private IAssemblySigner _signer;

    private Regex _regex = new Regex ("(.*)Locked(.*)");
    private string _defAttributeName = "GeneratedAttributeAttribute";
    private string _defAttributeNamespace = "GeneratedAttributeSpace";

    [SetUp]
    public void SetUp ()
    {
      _verifier = PEVerifier.CreateDefault ();
      Directory.CreateDirectory (TempPath);
      foreach (var file in Directory.EnumerateFiles (AssemblyPath, "*.dll", SearchOption.TopDirectoryOnly))
      {
        Console.WriteLine (TempPath + file.Substring (file.IndexOf ("integration") +11));
        File.Copy (file, TempPath + file.Substring (file.IndexOf ("integration") +11), true);
      }

      var allFiles =
          Directory.EnumerateFiles (TempPath, "*.dll", SearchOption.AllDirectories)
              .Concat (Directory.EnumerateFiles (TempPath, "*.exe", SearchOption.AllDirectories));

      List<AssemblyDefinition> assemblies = allFiles.Select (AssemblyDefinition.ReadAssembly).ToList();

      _tracker = new AssemblyTracker (assemblies);

      var strategy = new GeneratedMarkingAttributeStrategy (_defAttributeNamespace, _defAttributeName);
      _transformator = new AssemblyMethodsVirtualizer.AssemblyMethodsVirtualizer(strategy, _regex);

      _signer = new AssemblySigner (new ModuleDefinitionWriter (new FileSystem.FileSystem(), null, new List<StrongNameKeyPair>()));
    }

    [Test]
    public void MarkMethods_NoKey ()
    {
      _transformator.Transform (_tracker);
      _signer.SignAndSave (_tracker);

      CheckVirtuality ();
    }

    [Test]
    public void MarkMethods_WithKey ()
    {
      _signer = new AssemblySigner (new ModuleDefinitionWriter (new FileSystem.FileSystem (), AssemblyNameReferenceObjectMother.RealKeyPair(), new List<StrongNameKeyPair> ()));

      _transformator.Transform (_tracker);
      _signer.SignAndSave (_tracker);

      CheckVirtuality ();
    }

    private void CheckVirtuality ()
    {
      var allFiles = Directory.EnumerateFiles (TempPath, "*.dll", SearchOption.AllDirectories)
                    .Concat (Directory.EnumerateFiles (TempPath, "*.exe", SearchOption.AllDirectories));
      List<AssemblyDefinition> assemblies = allFiles.Select (AssemblyDefinition.ReadAssembly).ToList ();

      foreach (var assm in assemblies)
      {
        var targets = from moduleDefinition in assm.Modules
                      from typeDefinition in moduleDefinition.Types
                      from methodDefinition in typeDefinition.Methods
                      where _regex.IsMatch (methodDefinition.FullName)
                      select new { Assembly = assm, Method = methodDefinition };
        foreach (var target in targets)
        {
          Assert.That (target.Method.IsVirtual);
          Assert.That (target.Method.CustomAttributes[0].AttributeType.Name == _defAttributeName);
          Assert.That (target.Method.CustomAttributes[0].AttributeType.Namespace == _defAttributeNamespace);
        }
      }

    }

    [TearDown]
    public void TearDown ()
    {
      foreach (var file in Directory.EnumerateFiles (TempPath, "*.dll", SearchOption.AllDirectories))
        _verifier.VerifyPEFile (file);

      foreach (var file in Directory.EnumerateFiles (TempPath, "*", SearchOption.AllDirectories))
        File.Delete (file);
      //Directory.Delete (TempPath, true);
    }
  }
}