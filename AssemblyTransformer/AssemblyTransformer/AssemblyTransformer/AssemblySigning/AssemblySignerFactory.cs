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
    // TODO Review FS: Rename to _defaultKeyFile and _allKeysDirectory
    private string _defaultKey;
    private string _allKeys;

    public AssemblySignerFactory (IFileSystem fileSystem)
    {
      _fileSystem = fileSystem;
    }

    // TODO Review FS: Why so many different names for the same command line parameter?
    public void AddOptions (OptionSet options)
    {
      options.Add (
            "k|key|defaultKey=", 
            "The default key (.snk) to be used to sign Assemblies.",
            key => _defaultKey = key);
      // TODO Review FS: Why "s"?
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
          defaultKeyPair = ReadStrongNameKeyPair (defaultKey);
          Console.WriteLine ("Using keypair [" + defaultKey + "] as default signing key.");
        }
        catch (Exception e) // TODO Review FS: Catch IOException rather than Exception - be as specific as possible when catching exceptions; otherwise you might catch OutOfMemoryExceptions and similar, which shouldn't be handled that way
        {
          // TODO Review FS: Include the IOException's message in the new exception (and maybe supply it as the inner exception of the new exception) to provide more information
          // TODO Review FS: Consider using a custom exception (eg., OptionsException) to notify the program of an invalid option
          throw new ArgumentException ("The defaultKey file could not be opened!");
        }
      }
      if (keysDirectory != null)
      {
        try
        {
          availableKeyPairs.AddRange (_fileSystem.EnumerateFiles (keysDirectory, "*.snk", SearchOption.AllDirectories).Select (ReadStrongNameKeyPair));
          Console.WriteLine ("Using keys in [" + keysDirectory + "] to resign assemblies.");
        }
        catch (Exception e) // TODO Review FS: Catch IOException rather than Exception - be as specific as possible when catching exceptions; otherwise you might catch OutOfMemoryExceptions and similar, which shouldn't be handled that way
        {
          // TODO Review FS: Include the IOException's message in the new exception (and maybe supply it as the inner exception of the new exception) to provide more information
          // TODO Review FS: Consider using a custom exception (eg., OptionsException) to notify the program of an invalid option
          throw new ArgumentException ("Reading keys from the key directory caused an error!");
        }
      }
      
      // TODO Review FS: Do not create a new FileSystem, reuse _fileSystem
      return new ModuleWriter (new FileSystem.FileSystem (), defaultKeyPair, availableKeyPairs);
    }

    private StrongNameKeyPair ReadStrongNameKeyPair (string filePath)
    {
      // TODO Review FS: Use a "using" block to ensure the opened file is closed when finished with it. (In the test expect that the stream returned by Open is closed when the method is finished.)
      return new StrongNameKeyPair (_fileSystem.Open (filePath, FileMode.Open));
    }
  }
}