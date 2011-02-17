// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.FileSystem;

namespace AssemblyTransformer.AssemblyTransformations.AssemblyTransformationFactoryFactory
{
  public class DLLBasedTransformationFactoryFactory : ITransformationFactoryFactory
  {
    private readonly IFileSystem _fileSystem;

    private string _workingDirectory;
    public string WorkingDirectory
    {
      get { return _workingDirectory; }
      set { _workingDirectory = value; }
    }

    public DLLBasedTransformationFactoryFactory (IFileSystem fileSystem)
    {
      _fileSystem = fileSystem;
    }

    public void AddOptions (OptionSet options)
    {
      ArgumentUtility.CheckNotNull ("options", options);

      options.Add (
          "tdir|transformations=",
          "The (root) directory containing the transformation assemblies.",
          dir => _workingDirectory = dir);
    }

    public ICollection<IAssemblyTransformationFactory> CreateTrackerFactories ()
    {
      if (_workingDirectory == null)
        throw new InvalidOperationException ("Initialize options first.");

      var allFiles = _fileSystem.EnumerateFiles (_workingDirectory, "*.dll", SearchOption.AllDirectories);
      var factoryInterface = typeof (IAssemblyTransformationFactory);
      ICollection<IAssemblyTransformationFactory> transformationFactories = new List<IAssemblyTransformationFactory> ();

      foreach (var file in allFiles)
      {
        var assembly = _fileSystem.LoadAssemblyFrom (file);
        foreach (var type in assembly.GetTypes())
        {
          if (factoryInterface.IsAssignableFrom (type))
          {
            var transformationFactory = (IAssemblyTransformationFactory) Activator.CreateInstance (type, _fileSystem);
            transformationFactories.Add (transformationFactory);
          }
        }
      }
      return transformationFactories;
    }
  }
}