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
  // TODO Review FS: Rename to ModuleDefinitionWriter for consistency
  public class ModuleWriter : IModuleDefinitionWriter
  {
    private readonly IFileSystem _fileSystem;
    private readonly StrongNameKeyPair _defaultKey;
    private readonly List<StrongNameKeyPair> _keys;

    // TODO Review FS: Consider passing in IEnumerable<StringNameKeyPair> - that's more general - and then storing it in an StrongNameKeyPair[]; the List<...> is not really needed because you don't add or remove items
    public ModuleWriter (IFileSystem fileSystem, StrongNameKeyPair defaultKey, List<StrongNameKeyPair> availableKeys)
    {
      _fileSystem = fileSystem;
      _defaultKey = defaultKey;
      _keys = availableKeys;
    }

    // TODO Review FS: Is it necessary to pass in the StrongNameKeyPair when saving a secondary module? I'd guess no because the module has no manifest anyway, right? If the key pair is not required when saving a secondary module, you could omit the mainModule parameter because you only need to find the matching key if the moduleDefinition's IsMain property is true. And in that case, moduleDefinition.Assembly should be usable. As I said, I'm only guessing :)
    // TODO Review FS: If you need to keep the separate parameter, I would pass in the assembly or the assembly name, not the main module because the main module is only used to get the assembly name
    public void WriteModule (ModuleDefinition mainModule, ModuleDefinition moduleDefinition)
    {
      StrongNameKeyPair signKey = null;
      if (mainModule.Assembly.Name.HasPublicKey)
        signKey = GetKey (_keys, mainModule.Assembly.Name.PublicKey);
      
      // TODO Review FS: I wouldn't pass in a key if the assembly doesn't already have a public key. Ie., if you write a module of an unsigned assembly, no key pair should be used. Only use the default key for signed assembly where no key pair can be found.
      if (signKey == null)
        signKey = _defaultKey;

      Write (moduleDefinition, signKey);
    }

    private void Write (ModuleDefinition moduleDefinition, StrongNameKeyPair signKey)
    {
      // TODO Review FS: Consider adding support for the case where the bak file already exists (add a counter to the file name that is incremented while a file with that name exists; requires IFileSystem.FileExists method)
      // TODO Review FS: Consider adding support for the case where the module file does not yet exist (no Move should be performed in this case; requires IFileSystem.FileExists method) - this would allow the writer to be used for modules generated in memory
      _fileSystem.Move (moduleDefinition.FullyQualifiedName, moduleDefinition.FullyQualifiedName + ".bak");
      // TODO Review FS: Does Cecil automatically change the module's assembly's public key if a different StrongNameKeyPair is supplied? If yes, document this here in a comment - this is important for understanding the code.
      _fileSystem.WriteModuleDefinition (moduleDefinition, moduleDefinition.FullyQualifiedName, new WriterParameters { StrongNameKeyPair = signKey });
    }

    // TODO Review FS: Since _keys is a field, consider removing the parameter and using _keys instead.
    // TODO Review FS: Maybe rename to FindMatchingKeyPair because it implements a linear search, which is better expressed by "Find"
    private StrongNameKeyPair GetKey (List<StrongNameKeyPair> keys, byte[] publicKey)
    {
      return keys.FirstOrDefault (key => key.PublicKey.SequenceEqual (publicKey));
    }
  }
}