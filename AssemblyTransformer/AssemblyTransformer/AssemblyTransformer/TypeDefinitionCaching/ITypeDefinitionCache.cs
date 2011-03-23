// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;

namespace AssemblyTransformer.TypeDefinitionCaching
{
  public interface ITypeDefinitionCache
  {
    bool IsInitialized { get; }
    void InitializeCache (IAssemblyTracker tracker);
    Tuple<TypeDefinition, AssemblyDefinition> this[TypeReference typ, AssemblyNameReference assembly] { get; }
    Tuple<TypeDefinition, AssemblyDefinition> this[TypeReference typ, AssemblyDefinition assembly] { get; }
    Tuple<TypeDefinition, AssemblyDefinition> this[string typeAssemblyQualifiedName] { get; }
  }
}