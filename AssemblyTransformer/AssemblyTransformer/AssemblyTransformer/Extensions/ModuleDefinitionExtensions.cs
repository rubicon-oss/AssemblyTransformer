// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Collections.Generic;
using Mono.Cecil;

namespace AssemblyTransformer.Extensions
{
  public static class ModuleDefinitionExtensions
  {
    /// <summary>
    /// Loads all types from the module, including nested types (nested types are resolved recursively)
    /// </summary>
    public static IEnumerable<TypeDefinition> LoadAllTypes (this ModuleDefinition module)
    {
      var allTypes = new List<TypeDefinition> ();
      foreach (var typ in module.Types)
      {
        allTypes.Add (typ);
        allTypes.AddRange (typ.LoadAllNestedTypes ());
      }
      return allTypes;
    }
  }
}