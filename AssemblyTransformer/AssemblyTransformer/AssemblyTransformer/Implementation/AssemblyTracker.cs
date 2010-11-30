// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using System.Linq;

namespace AssemblyTransformer
{
  public class AssemblyTracker : IAssemblyTracker
  {
    private readonly IEnumerable<AssemblyDefinition> _assemblyDefinitions;

    public AssemblyTracker (IEnumerable<AssemblyDefinition> assemblyDefinitions)
    {
      _assemblyDefinitions = assemblyDefinitions;
    }

    public IEnumerable<AssemblyDefinition> GetAssemblies ()
    {
      return _assemblyDefinitions;
    }

    public AssemblyDefinition GetAssemblyByReference (AssemblyNameReference referencedAssemblyName)
    {
      return _assemblyDefinitions.Single (asm => referencedAssemblyName.MatchesDefinition (asm.Name));
    }
  }
}