// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Collections.Generic;
using Mono.Cecil;

namespace AssemblyTransformer.Extensions
{
  public static class AssemblyDefinitionExtensions
  {
    /// <summary>
    /// Loads all types from the assembly, including nested types (nested types are resolved recursively)
    /// </summary>
    public static IEnumerable<TypeDefinition> LoadAllTypes (this AssemblyDefinition assembly)
    {
      var allTypes = new List<TypeDefinition> ();
      foreach (var module in assembly.Modules)
        allTypes.AddRange (module.LoadAllTypes());
      return allTypes;
    }
  }
}