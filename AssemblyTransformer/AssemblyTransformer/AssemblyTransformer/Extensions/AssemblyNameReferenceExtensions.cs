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

    /// <summary>
    /// Builds the assembly qualified name for the given type.
    /// </summary>
    public static string BuildAssemblyQualifiedName (this AssemblyNameReference assembly, TypeReference typeRef)
    {
      if (typeRef == null)
        return null;

      if (typeRef.Scope.MetadataScopeType == MetadataScopeType.AssemblyNameReference)
        return typeRef.FullName + ", " + typeRef.Scope.Name;
      return typeRef.FullName + ", " + assembly.FullName;
    }

    public static string BuildReflectionAssemblyQualifiedName (this AssemblyNameReference assembly, TypeReference typeRef)
    {
      if (typeRef == null)
        return null;
      var fullName = BuildFullTypeRefName (typeRef).Replace (",", "\\,");

      if (typeRef.Scope.MetadataScopeType == MetadataScopeType.AssemblyNameReference)
        return fullName + ", " + ((AssemblyNameReference)typeRef.Scope).FullName;
      return fullName + ", " + assembly.FullName;
    }

    private static string BuildFullTypeRefName (TypeReference typeRef)
    {
      if (typeRef.IsNested)
        return BuildFullTypeRefName (typeRef.DeclaringType) + "+" + (!string.IsNullOrEmpty (typeRef.Namespace) ? (typeRef.Namespace + ".") : "") + typeRef.Name;
      if (typeRef.IsGenericInstance)
        return typeRef.Namespace + "." + typeRef.Name;
      return typeRef.FullName;
    }
  }
}