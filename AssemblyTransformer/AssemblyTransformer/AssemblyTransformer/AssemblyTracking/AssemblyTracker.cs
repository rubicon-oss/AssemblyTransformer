// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyTransformer.Extensions;
using AssemblyTransformer.TypeDefinitionCaching;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblyTracking
{
  /// <summary>
  /// Implements an assembly tracker. Provides functionality to handle tracked assemblies and associate them with the assembly definitions.
  /// </summary>
  public class AssemblyTracker : IAssemblyTracker
  {
    private readonly IDictionary<AssemblyDefinition, TrackedAssembly> _trackedAssemblies;
    private readonly ITypeDefinitionCache _typeDefinitionCache;

    public AssemblyTracker (IEnumerable<AssemblyDefinition> assemblyDefinitions, ITypeDefinitionCache typeDefinitionCache)
    {
      _typeDefinitionCache = typeDefinitionCache;
      _trackedAssemblies = assemblyDefinitions
        .Select (asm => new TrackedAssembly (asm))
        .ToDictionary (trackedAsm => trackedAsm.AssemblyDefinition);

      var reverseReferences = from trackedAssembly in _trackedAssemblies.Values
                              from module in trackedAssembly.AssemblyDefinition.Modules
                              where LoadAllAttributesIntoMemory (module)
                              from reference in module.AssemblyReferences
                              from trackedAssembliesByReference in GetTrackedAssembliesByReference (reference)
                              where trackedAssembliesByReference != null
                              select new { Origin = trackedAssembly, Reference = trackedAssembliesByReference };

      foreach (var reverseReference in reverseReferences)
        reverseReference.Reference.AddReverseReference (reverseReference.Origin);
    }

    private bool LoadAllAttributesIntoMemory (ModuleDefinition module)
    {
      // touching the customattributes constructor args causes resolving -> update of references.
      if (module.Assembly != null)
        foreach (var customAttribute in module.Assembly.CustomAttributes)
          TouchArguments (customAttribute);

      foreach (var customAttribute in module.CustomAttributes)
        TouchArguments (customAttribute);

      foreach (var type in module.LoadAllTypes ())
      {
        foreach (var customAttribute in type.CustomAttributes)
          TouchArguments (customAttribute);

        foreach (var fieldDefinition in type.Fields)
          foreach (var customAttribute in fieldDefinition.CustomAttributes)
            TouchArguments (customAttribute);

        foreach (var eventDefinition in type.Events)
          foreach (var customAttribute in eventDefinition.CustomAttributes)
            TouchArguments (customAttribute);

        foreach (var genericParameter in type.GenericParameters)
          foreach (var customAttribute in genericParameter.CustomAttributes)
            TouchArguments (customAttribute);

        foreach (var methodDefinition in type.Methods)
        {
          foreach (var customAttribute in methodDefinition.CustomAttributes)
            TouchArguments (customAttribute);
          foreach (var parameterDefinition in methodDefinition.Parameters)
            foreach (var customAttribute in parameterDefinition.CustomAttributes)
              TouchArguments (customAttribute);
        }
      }
      return true;
    }

    private void TouchArguments (CustomAttribute attribute)
    {
      var tmp = attribute.ConstructorArguments;
    }

    public IEnumerable<AssemblyDefinition> GetAssemblies ()
    {
      return _trackedAssemblies.Keys;
    }

    public AssemblyDefinition[] GetAssembliesByReference (AssemblyNameReference referencedAssemblyName)
    {
      ArgumentUtility.CheckNotNull ("referencedAssemblyName", referencedAssemblyName);

      var trackedAssembly = GetTrackedAssembliesByReference (referencedAssemblyName);
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

    public void TrackNewReference (AssemblyDefinition originatingAssembly, AssemblyNameReference assemblyReference)
    {
      if (originatingAssembly.FullName == assemblyReference.FullName)
        return;
      var originatingTrackedAssembly = GetTrackedAssembly (originatingAssembly);
      var referencedTrackedAssemblies = GetTrackedAssembliesByReference (assemblyReference);
      foreach (var referencedTrackedAssembly in referencedTrackedAssemblies)
      {
        referencedTrackedAssembly.AddReverseReference (originatingTrackedAssembly);
      }
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

    public ITypeDefinitionCache TypeDefinitionCache
    {
      get
      {
        if (!_typeDefinitionCache.IsInitialized)
          _typeDefinitionCache.InitializeCache (this);
        return _typeDefinitionCache;
      }
    }

    private TrackedAssembly GetTrackedAssembly (AssemblyDefinition assemblyDefinition)
    {
      TrackedAssembly result;
      if (!_trackedAssemblies.TryGetValue (assemblyDefinition, out result))
        throw new ArgumentException ("The assembly '{0}' is not a tracked assembly.", assemblyDefinition.FullName);

      return result;
    }

    private TrackedAssembly[] GetTrackedAssembliesByReference (AssemblyNameReference referencedAssemblyName)
    {
      //return _trackedAssemblies.Values.SingleOrDefault (asm => referencedAssemblyName.MatchesDefinition (asm.AssemblyDefinition.Name));
      return _trackedAssemblies.Values.Where (asm => referencedAssemblyName.MatchesDefinition (asm.AssemblyDefinition.Name)).ToArray();
    }
  }
}