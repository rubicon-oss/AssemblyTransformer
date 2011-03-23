// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblyTracking
{
  /// <summary>
  /// the tracked assembly contains/wraps/decorates an assembly definition. The class provides more information 
  /// than an assembly definition alone. The references and dependencies can be traced, also the state of the
  /// assembly definition is saved.
  /// </summary>
  public class TrackedAssembly
  {
    private readonly AssemblyDefinition _assemblyDefinition;
    private readonly List<TrackedAssembly> _reverseReferences = new List<TrackedAssembly>();
    private bool _isModified = false;

    public TrackedAssembly (AssemblyDefinition assemblyDefinition)
    {
      ArgumentUtility.CheckNotNull ("assemblyDefinition", assemblyDefinition);

      _assemblyDefinition = assemblyDefinition;
    }

    public AssemblyDefinition AssemblyDefinition
    {
      get { return _assemblyDefinition; }
    }

    public bool IsModified
    {
      get { return _isModified; }
    }

    public ReadOnlyCollection<TrackedAssembly> ReverseReferences
    {
      get { return _reverseReferences.AsReadOnly(); }
    }

    public void AddReverseReference (TrackedAssembly reverseReference)
    {
      ArgumentUtility.CheckNotNull ("reverseReference", reverseReference);
      _reverseReferences.Add (reverseReference);
    }

    public void MarkModified ()
    {
      if (_isModified)
        return;
      
      _isModified = true;
      foreach (var reverseReference in ReverseReferences)
        reverseReference.MarkModified();
    }

    public void MarkUnmodified ()
    {
      _isModified = false;
    }
  }
}