// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using AssemblyTransformer.TypeDefinitionCaching;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblyTracking
{
  /// <summary>
  /// This interface has to be implemented by every assembly tracker. 
  /// </summary>
  public interface IAssemblyTracker
  {
    IEnumerable<AssemblyDefinition> GetAssemblies ();
    AssemblyDefinition[] GetAssembliesByReference (AssemblyNameReference referencedAssemblyName);
    bool IsModified (AssemblyDefinition assemblyDefinition);
    void MarkModified (AssemblyDefinition assemblyDefinition);
    void MarkUnmodified (AssemblyDefinition assemblyDefinition);
    void TrackNewReference (AssemblyDefinition originatingAssembly, AssemblyNameReference assemblyReference);
    AssemblyDefinition[] GetModifiedAssemblies ();
    AssemblyDefinition[] GetReverseReferences (AssemblyDefinition assemblyDefinition);
    ITypeDefinitionCache TypeDefinitionCache { get; }
  }
}