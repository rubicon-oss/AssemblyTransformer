// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using Mono.Cecil;
using System.Linq;

namespace AssemblyTransformer
{
  public static class AssemblyNameReferenceExtensions
  {
    public static bool MatchesDefinition (this AssemblyNameReference reference, AssemblyNameReference definition)
    {
      return  reference.Name == definition.Name 
          && (reference.Culture == null || reference.Culture == definition.Culture) 
          && (reference.Version == null || reference.Version == definition.Version)
          && (reference.PublicKeyToken == null || (definition.PublicKeyToken != null && reference.PublicKeyToken.SequenceEqual (definition.PublicKeyToken)));
    }

    public static AssemblyNameReference Clone (this AssemblyNameReference original)
    {
      return new AssemblyNameReference (original.Name, CloneIfNotNull(original.Version))
      {
        Attributes = original.Attributes,
        Culture = original.Culture,
        Hash = original.Hash,
        HashAlgorithm = original.HashAlgorithm,
        IsRetargetable = original.IsRetargetable,
        IsSideBySideCompatible = original.IsSideBySideCompatible,
        MetadataToken = original.MetadataToken,
        PublicKey = CloneIfNotNull(original.PublicKey),
        PublicKeyToken = CloneIfNotNull(original.PublicKeyToken)
      };
    }

    private static T CloneIfNotNull<T> (T originalValue) where T : class, ICloneable
    {
      return originalValue != null ? (T) originalValue.Clone() : null;
    }
  }
}