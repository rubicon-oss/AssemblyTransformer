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

namespace AssemblyTransformer.AssemblySigning
{
  /// <summary>
  /// The signer factory takes care of instantiating a filesystem and instantiating the signer. Also the sign keys
  /// are read and initialized for further usage.
  /// </summary>
  public class AssemblySignerFactory : IAssemblySignerFactory
  {
    private readonly IFileSystem _fileSystem;
    private string _defaultKeyFile;
    private string _allKeysDirectory;

    public AssemblySignerFactory (IFileSystem fileSystem)
    {
      ArgumentUtility.CheckNotNull ("fileSystem", fileSystem);

      _fileSystem = fileSystem;
    }

    // TODO Review FS: Why so many different names for the same command line parameter?
    public void AddOptions (OptionSet options)
    {
      ArgumentUtility.CheckNotNull ("options", options);

      options.Add (
            "k|key|defaultKey=", 
            "The default key (.snk) to be used to sign Assemblies.",
            key => _defaultKeyFile = key);
      // TODO Review FS: Why "s"?
      options.Add (
            "s|keyDir|keyDirectory=", 
            "The root dir of all keys (.snk) to sign Assemblies.",
            allKeys => _allKeysDirectory = allKeys);
    }

    public IAssemblySigner CreateSigner ()
    {
      return new AssemblySigner (CreateWriter(_defaultKeyFile, _allKeysDirectory));
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
        catch (IOException e)
        {
          // TODO Review FS: Consider using a custom exception (eg., OptionsException) to notify the program of an invalid option
          // REMARK isnt an option an argument?
          throw new ArgumentException ("The defaultKey file could not be opened! (" + e.Message +")", e);
        }
      }
      if (keysDirectory != null)
      {
        try
        {
          availableKeyPairs.AddRange (_fileSystem.EnumerateFiles (keysDirectory, "*.snk", SearchOption.AllDirectories).Select (ReadStrongNameKeyPair));
          Console.WriteLine ("Using keys in [" + keysDirectory + "] to resign assemblies.");
        }
        catch (IOException e) 
        {
          // TODO Review FS: Consider using a custom exception (eg., OptionsException) to notify the program of an invalid option
          throw new ArgumentException ("Reading keys from the key directory caused an error! (" + e.Message + ")", e);
        }
      }

      return new ModuleDefinitionWriter (_fileSystem, defaultKeyPair, availableKeyPairs);
    }

    private StrongNameKeyPair ReadStrongNameKeyPair (string filePath)
    {
      using (FileStream keyFile = _fileSystem.Open (filePath, FileMode.Open))
      {
        return new StrongNameKeyPair (keyFile);
      }

      // TODO Review FS: Use a "using" block to ensure the opened file is closed when finished with it. (In the test expect that the stream returned by Open is closed when the method is finished.)
    }
  }
}