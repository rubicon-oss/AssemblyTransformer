// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer;
using AssemblyTransformer.AssemblyTransformations;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;

namespace GenericAttributeGenerator
{
  public class GenericAttributeGeneratorFactory : IAssemblyTransformationFactory
  {
    private readonly IFileSystem _fileSystem;

    private string _directory;

    public GenericAttributeGeneratorFactory (IFileSystem fileSystem)
    {
      ArgumentUtility.CheckNotNull ("fileSystem", fileSystem);
      _fileSystem = fileSystem;
    }

    public void AddOptions (OptionSet options)
    {
      ArgumentUtility.CheckNotNull ("options", options);

      options.Add (
             "d|dir|directory=",
             "The (root) directory containing the targeted assemblies.",
             d => _directory = d);
      }

    public IAssemblyTransformation CreateTransformation ()
    {
      if (_directory == null)
        throw new InvalidOperationException ("Initialize options first! (For GenericAttributeGenerator)");

      return new GenericAttributeGenerator (typeof(GenericAttributeMarkerAttribute));
    }
  }
}