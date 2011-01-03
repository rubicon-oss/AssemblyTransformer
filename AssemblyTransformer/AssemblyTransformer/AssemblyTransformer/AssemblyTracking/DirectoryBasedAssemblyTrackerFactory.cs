// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;
using Mono.Options;
using System.Linq;

namespace AssemblyTransformer.AssemblyTracking
{
  public class DirectoryBasedAssemblyTrackerFactory : IAssemblyTrackerFactory
  {
    private readonly IFileSystem _fileSystem;
    private string _workingDirectory;

    public DirectoryBasedAssemblyTrackerFactory (IFileSystem fileSystem)
    {
      _fileSystem = fileSystem;
    }

    public void AddOptions (OptionSet options)
    {
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
         catch (Exception) // TODO Review FS: If Cecil throws a sensible exception type when a file is read that is not a .NET assembly, catch that exception type instead
         {
           // TODO Review FS: Use the original exception message in the warning - there are many different cases why Cecil can refuse reading an assembly.
           Console.WriteLine ("    WARNING :: " + doc + " is not a .NET assembly!");
        }
      }

      return new AssemblyTracker (assemblies);
    }

  }
}