// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using AssemblyTransformer.FileSystem;

namespace AssemblyTransformer.AssemblyTransformations.AssemblyTransformationFactoryFactory
{
  public class DLLBasedTransformationFactoryFactory : ITransformationFactoryFactory
  {
    private readonly IFileSystem _fileSystem;
    private string _transformationsDirectory;
    private string _workingDirectory;
    private readonly HashSet<string> _transformations;

    public string WorkingDirectory
    {
      get { return _workingDirectory; }
      set { _workingDirectory = value; }
    }

    public string TransformationsDirectory
    {
      get { return _transformationsDirectory; }
      set { _transformationsDirectory = value; }
    }

    public DLLBasedTransformationFactoryFactory (IFileSystem fileSystem, string workingDir)
    {
      _fileSystem = fileSystem;
      _transformations = new HashSet<string>();
      _workingDirectory = workingDir;
    }

    public void AddOptions (OptionSet options)
    {
      ArgumentUtility.CheckNotNull ("options", options);

      options.Add (
          "t|transformation=",
          "The filename(s) of the transformations to be executed. (e.g.: '-t=Virtualizer.dll -t=Constructor')",
          transformation => {
            if (transformation.EndsWith (".dll"))
              _transformations.Add (transformation);
            else
              _transformations.Add (transformation + ".dll");
          });
    }

    public ICollection<IAssemblyTransformationFactory> CreateTrackerFactories ()
    {
      if (_transformationsDirectory == null)
        throw new InvalidOperationException ("Initialize options first. (workingdir on DLLBasedTransformationFactoryFactory)");

      //var allFiles = _fileSystem.EnumerateFiles (_transformationsDirectory, "*.dll", SearchOption.AllDirectories);
      var factoryInterface = typeof (IAssemblyTransformationFactory);
      ICollection<IAssemblyTransformationFactory> transformationFactories = new List<IAssemblyTransformationFactory> ();

      foreach (var file in _transformations)
      {
        try
        {
          var assembly = _fileSystem.LoadAssemblyFrom (Path.Combine(_transformationsDirectory, file));
          foreach (var type in assembly.GetTypes ())
          {
            if (factoryInterface.IsAssignableFrom (type))
            {
              var transformationFactory = (IAssemblyTransformationFactory) Activator.CreateInstance (type, _fileSystem, _workingDirectory);
              transformationFactories.Add (transformationFactory);
            }
          }
        }
        catch (Exception e)
        {
          Console.WriteLine ("Could not load " + file + " " + e);
        }
      }
      return transformationFactories;
    }
  }
}