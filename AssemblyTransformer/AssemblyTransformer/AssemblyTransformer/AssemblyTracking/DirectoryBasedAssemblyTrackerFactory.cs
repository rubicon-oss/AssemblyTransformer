// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
//#define WRITE_OUTPUT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssemblyTransformer.FileSystem;
using AssemblyTransformer.TypeDefinitionCaching;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblyTracking
{
  /// <summary>
  /// This assembly tracker factory implementation creates a directory based tracker. The user has to specify the root directory.
  /// All dlls and exes are loaded. In case of an error, a warning is printed, but the process still commences!
  /// </summary>
  public class DirectoryBasedAssemblyTrackerFactory : IAssemblyTrackerFactory
  {
    private readonly IFileSystem _fileSystem;
    private List<string> _whiteList;
    private readonly List<string> _blackList;
    private readonly string _workingDirectory;
    private SearchOption _includeSubDirs = SearchOption.TopDirectoryOnly;

    public List<string> IncludeFiles { 
      get { return _whiteList; } 
      set { _whiteList = value; } 
    }

      public DirectoryBasedAssemblyTrackerFactory (IFileSystem fileSystem, string workingDirectory)
    {
      ArgumentUtility.CheckNotNull ("fileSystem", fileSystem);

      _workingDirectory = workingDirectory;
      _fileSystem = fileSystem;
      _blackList = new List<string> ();
    }

    public void AddOptions (OptionSet options)
    {
      ArgumentUtility.CheckNotNull ("options", options);

      options.Add (
          "e|exclude=",
          "The targeted assemblies (eg: -b=Remotion.Interfaces.dll -b=SomeLibrary.*.dll)",
          b => _blackList.Add (b));
      options.Add (
          "s|subdirs",
          "Include subdirectories of the workingdir.",
          b => _includeSubDirs = (b != null ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
    }

    public IAssemblyTracker CreateTracker ()
    {
      if (_whiteList == null || _whiteList.Count == 0)
        throw new InvalidOperationException
          ("Initialize options first. (AssemblyTracker: target files have to be set!)");

      var readPDB = new ReaderParameters { ReadSymbols = true };
      var ignorePDB = new ReaderParameters { ReadSymbols = false };
      var allFiles = BuildTargetFilesList();

      List<AssemblyDefinition> assemblies = new List<AssemblyDefinition> ();
      foreach (var doc in allFiles)
      {
        try
         {
           if (_fileSystem.FileExists (doc.Substring (0, doc.Length-3) + "pdb"))
             assemblies.Add (_fileSystem.ReadAssembly (doc, readPDB));
           else
             assemblies.Add (_fileSystem.ReadAssembly (doc, ignorePDB));

         }
         catch (BadImageFormatException e)
         {
           Console.WriteLine ("   WARNING :: '" + doc + "' is not a valid .NET assembly! [is ignored]");
         }
      }
      ((BaseAssemblyResolver) GlobalAssemblyResolver.Instance).AddSearchDirectory (_workingDirectory);
      return new AssemblyTracker (assemblies, new TypeDefinitionCache());
    }

    private IEnumerable<string> BuildTargetFilesList ()
    {
      var tmpList = new List<string> ();
      var tmpBlackList = new List<string> ();

      foreach (var target in _whiteList)
        tmpList.AddRange (_fileSystem.EnumerateFiles (_workingDirectory, target, _includeSubDirs));

      foreach (var nonTarget in _blackList)
        tmpBlackList.AddRange (_fileSystem.EnumerateFiles (_workingDirectory, nonTarget, _includeSubDirs));

      return tmpList.Except (tmpBlackList);
    }

  }
}