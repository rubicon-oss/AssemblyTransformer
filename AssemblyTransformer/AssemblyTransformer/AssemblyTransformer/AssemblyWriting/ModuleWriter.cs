// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AssemblyTransformer.AssemblySigning;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblyWriting
{
  public class ModuleWriter : IModuleDefinitionWriter
  {

    public void WriteModule (ModuleDefinition moduleDefinition)
    {
      Write (moduleDefinition, null);
    }

    public void WriteModule (ModuleDefinition mainModule, ModuleDefinition moduleDefinition, 
                              List<StrongNameKeyPair> keys, StrongNameKeyPair defaultKey)
    {
      StrongNameKeyPair signKey = null;
      if (mainModule.Assembly.Name.HasPublicKey)
        signKey = GetKey (keys, mainModule.Assembly.Name.PublicKey);
      if (signKey == null)
        signKey = defaultKey;
      Write (moduleDefinition, signKey);
    }

    private void Write (ModuleDefinition moduleDefinition, StrongNameKeyPair signKey)
    {
      int suffixPos = moduleDefinition.FullyQualifiedName.LastIndexOf ('.');
      moduleDefinition.Write (moduleDefinition.FullyQualifiedName.Substring (0, suffixPos) + ".modif" + 
                              moduleDefinition.FullyQualifiedName.Substring (suffixPos), 
                              new WriterParameters { StrongNameKeyPair = signKey });
    }

    private StrongNameKeyPair GetKey (List<StrongNameKeyPair> keys, byte[] publicKey)
    {
      return keys.FirstOrDefault (key => key.PublicKey.SequenceEqual (publicKey));
    }

  }
}