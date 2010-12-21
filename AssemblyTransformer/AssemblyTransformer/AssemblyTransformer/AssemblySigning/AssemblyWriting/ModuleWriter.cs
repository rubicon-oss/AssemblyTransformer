// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblySigning.AssemblyWriting
{
  public class ModuleWriter : IModuleDefinitionWriter
  {
    private readonly IFileSystem _fileSystem;
    private readonly StrongNameKeyPair _defaultKey;
    private readonly List<StrongNameKeyPair> _keys;

    public ModuleWriter (IFileSystem fileSystem, StrongNameKeyPair defaultKey, List<StrongNameKeyPair> availableKeys)
    {
      _fileSystem = fileSystem;
      _defaultKey = defaultKey;
      _keys = availableKeys;
    }

    public void WriteModule (ModuleDefinition mainModule, ModuleDefinition moduleDefinition)
    {
      StrongNameKeyPair signKey = null;
      if (mainModule.Assembly.Name.HasPublicKey)
        signKey = GetKey (_keys, mainModule.Assembly.Name.PublicKey);
      if (signKey == null)
        signKey = _defaultKey;
      Write (moduleDefinition, signKey);
    }

    private void Write (ModuleDefinition moduleDefinition, StrongNameKeyPair signKey)
    {
      _fileSystem.Move (moduleDefinition.FullyQualifiedName, moduleDefinition.FullyQualifiedName + ".bak");
      _fileSystem.WriteModuleDefinition (moduleDefinition, moduleDefinition.FullyQualifiedName, new WriterParameters { StrongNameKeyPair = signKey });
    }

    private StrongNameKeyPair GetKey (List<StrongNameKeyPair> keys, byte[] publicKey)
    {
      return keys.FirstOrDefault (key => key.PublicKey.SequenceEqual (publicKey));
    }

  }
}