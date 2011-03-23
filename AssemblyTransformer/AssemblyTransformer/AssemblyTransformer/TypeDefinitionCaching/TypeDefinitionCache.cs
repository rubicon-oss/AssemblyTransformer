// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.Extensions;
using Mono.Cecil;

namespace AssemblyTransformer.TypeDefinitionCaching
{
  public class TypeDefinitionCache : ITypeDefinitionCache
  {
    private readonly IDictionary<string, Tuple<TypeDefinition, AssemblyDefinition>> _cachedTypes;
    public bool IsInitialized { get; private set; }

    public TypeDefinitionCache ()
    {
      IsInitialized = false;
      _cachedTypes = new Dictionary<string, Tuple<TypeDefinition, AssemblyDefinition>> ();
    }

    public void InitializeCache (IAssemblyTracker tracker)
    {
      _cachedTypes.Clear();
      foreach (var assembly in tracker.GetAssemblies())
        foreach (var type in assembly.LoadAllTypes())
          _cachedTypes.Add (assembly.Name.BuildAssemblyQualifiedName (type), Tuple.Create (type, assembly));
      IsInitialized = true;
    }

    public Tuple<TypeDefinition, AssemblyDefinition> this[string typeAssemblyQualifiedNameAssemblyName]
    {
      get
      {
        return _cachedTypes.ContainsKey (typeAssemblyQualifiedNameAssemblyName) ? _cachedTypes[typeAssemblyQualifiedNameAssemblyName] : null;
      }
    }

    public Tuple<TypeDefinition, AssemblyDefinition> this[TypeReference typ, AssemblyNameReference assembly]
    {
      get
      {
        return typ == null ? null : this[assembly.BuildAssemblyQualifiedName (typ)];
      }
    }

    public Tuple<TypeDefinition, AssemblyDefinition> this[TypeReference typ, AssemblyDefinition assembly]
    {
      get
      {
        return this[typ, assembly.Name];
      }
    }
  }
}