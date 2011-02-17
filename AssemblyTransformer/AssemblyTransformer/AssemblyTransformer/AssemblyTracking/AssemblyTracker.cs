// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyTransformer.Extensions;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblyTracking
{
  /// <summary>
  /// Implements an assembly tracker. Provides functionality to handle tracked assemblies and associate them with the assembly definitions.
  /// </summary>
  public class AssemblyTracker : IAssemblyTracker
  {
    private readonly IDictionary<AssemblyDefinition, TrackedAssembly> _trackedAssemblies;

    public AssemblyTracker (IEnumerable<AssemblyDefinition> assemblyDefinitions)
    {
      _trackedAssemblies = assemblyDefinitions
        .Select (asm => new TrackedAssembly (asm))
        .ToDictionary (trackedAsm => trackedAsm.AssemblyDefinition);

      var reverseReferences = from trackedAssembly in _trackedAssemblies.Values
                              from module in trackedAssembly.AssemblyDefinition.Modules
                              from reference in module.AssemblyReferences
                              //let trackedAssemblyByReference = GetTrackedAssemblyByReference (reference)
                              from trackedAssembliesByReference in GetTrackedAssemblyByReference (reference)
                              where trackedAssembliesByReference != null
                              select new { Origin = trackedAssembly, Reference = trackedAssembliesByReference };

      foreach (var reverseReference in reverseReferences)
        reverseReference.Reference.AddReverseReference (reverseReference.Origin);
    }

    public IEnumerable<AssemblyDefinition> GetAssemblies ()
    {
      return _trackedAssemblies.Keys;
    }

    public AssemblyDefinition[] GetAssemblyByReference (AssemblyNameReference referencedAssemblyName)
    {
      ArgumentUtility.CheckNotNull ("referencedAssemblyName", referencedAssemblyName);

      var trackedAssembly = GetTrackedAssemblyByReference (referencedAssemblyName);
        foreach (var assembly in trackedAssembly)
        {
          Console.WriteLine ("## " + assembly.AssemblyDefinition.Name);
        }
      return trackedAssembly != null ? trackedAssembly.Select(asm => asm.AssemblyDefinition).ToArray() : null;
    }

    public bool IsModified (AssemblyDefinition assemblyDefinition)
    {
      ArgumentUtility.CheckNotNull ("assemblyDefinition", assemblyDefinition);

      var trackedAssembly = GetTrackedAssembly (assemblyDefinition);
      return trackedAssembly.IsModified;
    }

    public void MarkModified (AssemblyDefinition assemblyDefinition)
    {
      ArgumentUtility.CheckNotNull ("assemblyDefinition", assemblyDefinition);

      var trackedAssembly = GetTrackedAssembly(assemblyDefinition);
      trackedAssembly.MarkModified();
    }

    public void MarkUnmodified (AssemblyDefinition assemblyDefinition)
    {
      ArgumentUtility.CheckNotNull ("assemblyDefinition", assemblyDefinition);

      var trackedAssembly = GetTrackedAssembly (assemblyDefinition);
      trackedAssembly.MarkUnmodified ();
    }

    public AssemblyDefinition[] GetModifiedAssemblies ()
    {
      return _trackedAssemblies.Values.Where (asm => asm.IsModified).Select(asm => asm.AssemblyDefinition).ToArray();
    }

    public AssemblyDefinition[] GetReverseReferences (AssemblyDefinition assemblyDefinition)
    {
      ArgumentUtility.CheckNotNull ("assemblyDefinition", assemblyDefinition);

      var trackedAssembly = GetTrackedAssembly(assemblyDefinition);
      return trackedAssembly.ReverseReferences.Select(asm => asm.AssemblyDefinition).ToArray();
    }

    private TrackedAssembly GetTrackedAssembly (AssemblyDefinition assemblyDefinition)
    {
      TrackedAssembly result;
      if (!_trackedAssemblies.TryGetValue (assemblyDefinition, out result))
        throw new ArgumentException ("The assembly '{0}' is not a tracked assembly.", assemblyDefinition.FullName);

      return result;
    }

    private TrackedAssembly[] GetTrackedAssemblyByReference (AssemblyNameReference referencedAssemblyName)
    {
      //return _trackedAssemblies.Values.SingleOrDefault (asm => referencedAssemblyName.MatchesDefinition (asm.AssemblyDefinition.Name));
      return _trackedAssemblies.Values.Where (asm => referencedAssemblyName.MatchesDefinition (asm.AssemblyDefinition.Name)).ToArray();
    }
  }
}