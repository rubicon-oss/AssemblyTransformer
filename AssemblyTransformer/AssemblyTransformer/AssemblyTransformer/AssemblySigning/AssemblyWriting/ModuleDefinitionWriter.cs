// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblySigning.AssemblyWriting
{
  /// <summary>
  /// The module definition writer is a class, which takes care of writing the given modules to the filesystem.
  /// The newly created/modified assemblies will be saved in the same folder structure as the originals. The original
  /// files will be renamed (suffix .bak + count) and not deleted. In memory generated assemblies have to have a specified path!
  /// </summary>
  public class ModuleDefinitionWriter : IModuleDefinitionWriter
  {
    private readonly IFileSystem _fileSystem;
    private readonly StrongNameKeyPair _defaultKey;
    private readonly StrongNameKeyPair[] _keys;

    public StrongNameKeyPair DefaultKey
    {
      get { return _defaultKey; }
    }

    public StrongNameKeyPair[] Keys
    {
      get { return _keys; }
    }

    public ModuleDefinitionWriter (IFileSystem fileSystem, StrongNameKeyPair defaultKey, IEnumerable<StrongNameKeyPair> availableKeys)
    {
      ArgumentUtility.CheckNotNull ("fileSystem", fileSystem);
      ArgumentUtility.CheckNotNull ("availableKeys", availableKeys);

      _fileSystem = fileSystem;
      _defaultKey = defaultKey;
      _keys = availableKeys.ToArray();
    }

    public void WriteModule (ModuleDefinition moduleDefinition)
    {
      ArgumentUtility.CheckNotNull ("moduleDefinition", moduleDefinition);
      var writerParameters = new WriterParameters { WriteSymbols = moduleDefinition.HasSymbols };
      StrongNameKeyPair signKey = null;
      if (moduleDefinition.IsMain && 
        (moduleDefinition.Attributes & ModuleAttributes.StrongNameSigned) == ModuleAttributes.StrongNameSigned)
      {
        signKey = FindMatchingKeyPair (moduleDefinition.Assembly.Name.PublicKey);
        if (signKey == null)
        {
          signKey = _defaultKey;
          if (signKey == null)
          {
            moduleDefinition.Assembly.Name.HasPublicKey = false;
            moduleDefinition.Assembly.Name.PublicKey = new byte[0];
            moduleDefinition.Attributes &= ~ModuleAttributes.StrongNameSigned;
          }
        }
      }
      //if (signKey != null)
        writerParameters.StrongNameKeyPair = signKey;
      Write (moduleDefinition, writerParameters);
    }

    private void Write (ModuleDefinition moduleDefinition, WriterParameters writerParameters)
    {
      // uncomment to get backup files. (original files get .bak0, .bak1, etc. ending
      //if (_fileSystem.FileExists (moduleDefinition.FullyQualifiedName))
      //{
      //  int suffixCnt = 0;
      //  while (_fileSystem.FileExists (moduleDefinition.FullyQualifiedName + ".bak" + suffixCnt))
      //    ++suffixCnt;
      //  _fileSystem.Move (moduleDefinition.FullyQualifiedName, moduleDefinition.FullyQualifiedName + ".bak" + suffixCnt);
      //}
      // During building the assembly, an already existing sign key is replaced with the newly specified one!
      _fileSystem.WriteModuleDefinition (moduleDefinition, moduleDefinition.FullyQualifiedName, writerParameters);
    }

    private StrongNameKeyPair FindMatchingKeyPair (byte[] publicKey)
    {
      return _keys.FirstOrDefault (key => key.PublicKey.SequenceEqual (publicKey));
    }
  }
}