// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Collections.Generic;
using Mono.Cecil;

namespace AssemblyTransformer
{
  public interface IAssemblyTracker
  {

    IEnumerable<AssemblyDefinition> GetAssemblies ();
    AssemblyDefinition GetAssemblyByReference (AssemblyNameReference referencedAssemblyName);
    bool IsModified (AssemblyDefinition assemblyDefinition);
    void MarkModified (AssemblyDefinition assemblyDefinition);
    void MarkUnmodified (AssemblyDefinition assemblyDefinition);
    AssemblyDefinition[] GetModifiedAssemblies ();
    AssemblyDefinition[] GetReverseReferences (AssemblyDefinition assemblyDefinition);

  }
}