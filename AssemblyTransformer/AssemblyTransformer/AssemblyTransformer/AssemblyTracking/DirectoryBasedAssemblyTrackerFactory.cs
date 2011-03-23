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
    private readonly List<string> _whiteList;
    private readonly List<string> _blackList;
    private string _workingDirectory;

    public DirectoryBasedAssemblyTrackerFactory (IFileSystem fileSystem)
    {
      ArgumentUtility.CheckNotNull ("fileSystem", fileSystem);

      _fileSystem = fileSystem;
      _whiteList = new List<string> ();
      _blackList = new List<string> ();
    }

    public void AddOptions (OptionSet options)
    {
      ArgumentUtility.CheckNotNull ("options", options);

      options.Add (
          "d|dir=",
          "The (root) directory containing the targeted assemblies.",
          dir => _workingDirectory = dir);
      options.Add (
          "w|whitelist=",
          "The target assemblies. At least use: '-w=*.dll -w=*.exe'! (eg: '-w=Remotion.*.dll -w=ActaNova.*.dll -w=*.exe')",
          w => _whiteList.Add (w) );
      options.Add (
          "b|blacklist=",
          "The targeted assemblies (eg: -b=Remotion.Interfaces.dll -b=ActaNova.*.dll)",
          b => _blackList.Add (b));
    }

    public IAssemblyTracker CreateTracker ()
    {
      if (_workingDirectory == null || _whiteList.Count == 0)
        throw new InvalidOperationException 
          ("Initialize options first. (AssemblyTracker: workingDirectory AND a whitelist of files has to be present!)");

      var allFiles = BuildTargetFilesList();

      List<AssemblyDefinition> assemblies = new List<AssemblyDefinition> ();
      foreach (var doc in allFiles)
      {
#if WRITE_OUTPUT
         Console.WriteLine ("  processing " + doc + " ...");
#endif
         try
         {
            assemblies.Add (_fileSystem.ReadAssembly (doc));
         }
         catch (BadImageFormatException e)
         {
#if WRITE_OUTPUT
           Console.WriteLine ("    WARNING :: " + doc + " is not a .NET assembly! (" + e.Message + ")");
#endif
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
        tmpList.AddRange (_fileSystem.EnumerateFiles (_workingDirectory, target, SearchOption.AllDirectories));

      foreach (var nonTarget in _blackList)
        tmpBlackList.AddRange (_fileSystem.EnumerateFiles (_workingDirectory, nonTarget, SearchOption.AllDirectories));

      return tmpList.Except (tmpBlackList);
    }

  }
}