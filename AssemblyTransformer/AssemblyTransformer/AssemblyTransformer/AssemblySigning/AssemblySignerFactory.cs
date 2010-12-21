// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AssemblyTransformer.AssemblySigning.AssemblyWriting;
using AssemblyTransformer.FileSystem;
using Mono.Options;

namespace AssemblyTransformer.AssemblySigning
{
  public class AssemblySignerFactory : IAssemblySignerFactory
  {
    private readonly IFileSystem _fileSystem;
    private string _defaultKey;
    private string _allKeys;

    public AssemblySignerFactory (IFileSystem fileSystem)
    {
      _fileSystem = fileSystem;
    }

    public void AddOptions (OptionSet options)
    {
      options.Add (
            "k|key|defaultKey=", 
            "The default key (.snk) to be used to sign Assemblies.",
            key => _defaultKey = key);
      options.Add (
            "s|keyDir|keyDirectory=", 
            "The root dir of all keys (.snk) to sign Assemblies.",
            allKeys => _allKeys = allKeys);
    }

    public IAssemblySigner CreateSigner ()
    {
      return new AssemblySigner (CreateWriter(_defaultKey, _allKeys));
    }

    private IModuleDefinitionWriter CreateWriter (string defaultKey, string keysDirectory)
    {
      List<StrongNameKeyPair> availableKeyPairs = new List<StrongNameKeyPair> ();
      StrongNameKeyPair defaultKeyPair = null;
      if (defaultKey != null)
      {
        try
        {
          defaultKeyPair = new StrongNameKeyPair (_fileSystem.Open (defaultKey, FileMode.Open));
          Console.WriteLine ("Using keypair [" + defaultKey + "] as default signing key.");
        }
        catch (Exception e)
        {
          throw new ArgumentException ("The defaultKey file could not be opened!");
        }
      }
      if (keysDirectory != null)
      {
        try
        {
          availableKeyPairs.AddRange (_fileSystem.EnumerateFiles (keysDirectory, "*.snk", SearchOption.AllDirectories)
                                          .Select (keyPair => new StrongNameKeyPair (_fileSystem.Open (keyPair, FileMode.Open))));
          Console.WriteLine ("Using keys in [" + keysDirectory + "] to resign assemblies.");
        }
        catch (Exception e)
        {
          throw new ArgumentException ("Reading keys from the key directory caused an error!");
        }
      }
      return new ModuleWriter (new FileSystem.FileSystem (), defaultKeyPair, availableKeyPairs);
    }
  }
}