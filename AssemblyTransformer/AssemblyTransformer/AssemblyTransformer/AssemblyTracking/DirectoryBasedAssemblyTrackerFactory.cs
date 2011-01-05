// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssemblyTransformer.FileSystem;
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
    private string _workingDirectory;

    public DirectoryBasedAssemblyTrackerFactory (IFileSystem fileSystem)
    {
      ArgumentUtility.CheckNotNull ("fileSystem", fileSystem);

      _fileSystem = fileSystem;
    }

    public void AddOptions (OptionSet options)
    {
      ArgumentUtility.CheckNotNull ("options", options);

      options.Add (
          "d|dir|workingDirectory=",
          "The (root) directory containing the assemblies.",
          dir => _workingDirectory = dir);
    }

    public IAssemblyTracker CreateTracker ()
    {
      if (_workingDirectory == null)
        throw new InvalidOperationException ("Initialize options first.");

      var allFiles =
          _fileSystem.EnumerateFiles (_workingDirectory, "*.dll", SearchOption.AllDirectories)
              .Concat (_fileSystem.EnumerateFiles (_workingDirectory, "*.exe", SearchOption.AllDirectories));

      List<AssemblyDefinition> assemblies = new List<AssemblyDefinition> ();
      foreach (var doc in allFiles)
      {
         Console.WriteLine ("  processing " + doc + " ...");
         try
         {
            assemblies.Add (_fileSystem.ReadAssembly (doc));
         }
         catch (BadImageFormatException e)
         {
           Console.WriteLine ("    WARNING :: " + doc + " is not a .NET assembly! (" + e.Message + ")");
        }
      }

      return new AssemblyTracker (assemblies);
    }

  }
}