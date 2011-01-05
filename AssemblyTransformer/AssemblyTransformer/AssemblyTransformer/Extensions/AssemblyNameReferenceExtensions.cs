// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq;
using Mono.Cecil;

namespace AssemblyTransformer.Extensions
{
  /// <summary>
  /// These extensions provide necessary functionality to handle assembly name references, since Cecil does not, or insufficiently, provide these
  /// methods.
  /// </summary>
  public static class AssemblyNameReferenceExtensions
  {
    /// <summary>
    /// Compares the given references the way Microsoft intended it. (specified in ECMA#355)
    /// </summary>
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