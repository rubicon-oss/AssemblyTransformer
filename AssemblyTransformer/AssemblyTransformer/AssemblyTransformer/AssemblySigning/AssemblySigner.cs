// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblySigning
{
  public class AssemblySigner
  {
    private readonly IModuleDefinitionWriter _writer;

    public AssemblySigner (IModuleDefinitionWriter writer)
    {
      _writer = writer;
    }

    //public void SignAndSave (IAssemblyTracker tracker)
    //{
    //  foreach (var modifiedAssembly in tracker.GetModifiedAssemblies())
    //  {
    //    SignAndSave (tracker, modifiedAssembly);
    //  }
    //}

    //private void SignAndSave (IAssemblyTracker tracker, AssemblyDefinition modifiedAssembly)
    //{
    //  if (!tracker.IsModified (modifiedAssembly))
    //    return;

    //  foreach (var moduleDefinition in modifiedAssembly.Modules)
    //  {
    //    foreach (var nameReference in moduleDefinition.AssemblyReferences)
    //    {
    //      var referencedAssembly = tracker.GetAssemblyByReference (nameReference);
    //      SignAndSave (tracker, referencedAssembly);
    //    }
    //  }

    //  var oldName = AssemblyNameReference.Parse (modifiedAssembly.Name.FullName);
    //  _writer.WriteModule (modifiedAssembly.Modules);
    //  var newName = modifiedAssembly.Name;

    //  foreach (var reverseReference in tracker.GetReverseReferences (modifiedAssembly))
    //  {
    //    UpdateReferences (reverseReference, oldName, newName);
    //  }
    //}

    //private void UpdateReferences (AssemblyDefinition assemblyDefinition, AssemblyNameReference oldDefinition, AssemblyNameDefinition newDefinition)
    //{
    //  var nameReference = assemblyDefinition.MainModule.AssemblyReferences
    //      .Select ((reference, i) => new { Index = i, Reference = reference })
    //      .Single (tuple => tuple.Reference.MatchesDefinition (oldDefinition));
    //  assemblyDefinition.MainModule.AssemblyReferences[nameReference.Index] = newDefinition;
    //}
  }
}