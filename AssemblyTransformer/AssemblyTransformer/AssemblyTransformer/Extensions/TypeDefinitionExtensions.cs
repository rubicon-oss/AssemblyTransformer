// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Collections.Generic;
using Mono.Cecil;

namespace AssemblyTransformer.Extensions
{
  public static class TypeDefinitionExtensions
  {
    /// <summary>
    /// Loads all the nested types of the targetType recursively.
    /// </summary>
    public static IEnumerable<TypeDefinition> LoadAllNestedTypes (this TypeDefinition targetType)
    {
      var nestedTypes = new List<TypeDefinition> ();
      nestedTypes.AddRange (targetType.NestedTypes);
      foreach (var nestedType in targetType.NestedTypes)
        nestedTypes.AddRange (LoadAllNestedTypes (nestedType));
      return nestedTypes;
    }
  }
}