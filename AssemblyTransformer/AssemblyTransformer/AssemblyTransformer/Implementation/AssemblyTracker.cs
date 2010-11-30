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
    private readonly IDictionary<AssemblyDefinition, TrackedAssembly> _trackedAssemblies;

    public AssemblyTracker (IEnumerable<AssemblyDefinition> assemblyDefinitions)
    {
      _trackedAssemblies = assemblyDefinitions
        .Select (asm => new TrackedAssembly (asm))
        .ToDictionary (trackedAsm => trackedAsm.AssemblyDefinition);
    }

    public IEnumerable<AssemblyDefinition> GetAssemblies ()
    {
      return _trackedAssemblies.Keys;
    }

    public AssemblyDefinition GetAssemblyByReference (AssemblyNameReference referencedAssemblyName)
    {
      var assemblyDefinitions = GetAssemblies();
      return assemblyDefinitions.SingleOrDefault (asm => referencedAssemblyName.MatchesDefinition (asm.Name));
    }

    public void MarkModified (AssemblyDefinition assemblyDefinition)
    {
      var trackedAssembly = GetTrackedAssembly(assemblyDefinition);
      trackedAssembly.MarkModified();
    }

    public bool IsModified (AssemblyDefinition assemblyDefinition)
    {
      var trackedAssembly = GetTrackedAssembly (assemblyDefinition);
      return trackedAssembly.IsModified;
    }

    private TrackedAssembly GetTrackedAssembly (AssemblyDefinition assemblyDefinition)
    {
      TrackedAssembly result;
      if (!_trackedAssemblies.TryGetValue (assemblyDefinition, out result))
        throw new ArgumentException ("The assembly '{0}' is not a tracked assembly.", assemblyDefinition.FullName);
      
      return result;
    }
  }
}