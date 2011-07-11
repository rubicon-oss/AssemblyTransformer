using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyTransformer.AssemblySigning.AssemblyWriting;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.Extensions;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblySigning
{
  /// <summary>
  /// The assembly signer offers the core functionality of signing the given assemblies,
  /// recalculating the assemblies hashes/keys and modifying the depending assemblies
  /// references to contain the correct values again.
  /// This is a recursive algorithm, as every modification of an assembly can have sideeffects 
  /// on multiple other assemblies. The whole "assembly tree" is updated, and written using the
  /// given module definition writer.
  /// </summary>
  public class AssemblySigner : IAssemblySigner
  {
    private readonly IModuleDefinitionWriter _writer;

    public IModuleDefinitionWriter Writer
    {
      get { return _writer; }
    }

    public AssemblySigner (IModuleDefinitionWriter writer)
    {
      ArgumentUtility.CheckNotNull ("writer", writer);

      _writer = writer;
    }

    public void SignAndSave (IAssemblyTracker tracker)
    {
      ArgumentUtility.CheckNotNull ("tracker", tracker);

      ICollection<AssemblyDefinition> assembliesToSave = new List<AssemblyDefinition> (tracker.GetModifiedAssemblies ());
      while (assembliesToSave.Count != 0)
      {
        var modifiedAssembly = assembliesToSave.First();
        SignAndSave (tracker, modifiedAssembly, assembliesToSave);
        tracker.MarkUnmodified (modifiedAssembly);
      }
    }

    private void SignAndSave (IAssemblyTracker tracker, AssemblyDefinition assembly, ICollection<AssemblyDefinition> assembliesToSave)
    {
      // Remove this assembly from the list of assemblies to save before descending into recursive calls - this will avoid an endless loop with 
      // circular references.
      assembliesToSave.Remove (assembly);

      // Save all referenced assemblies before saving this assembly. Note that the referenced assemblies will automatically update this assembly's
      // references if they change their name while saving.
      foreach (var moduleDefinition in assembly.Modules)
      {
        foreach (var assemblyNameReference in moduleDefinition.AssemblyReferences)
        {
          var referencedAssembly = tracker.GetAssembliesByReference (assemblyNameReference);
          foreach (var adef in referencedAssembly)
          {
            if (assembliesToSave.Contains (adef))
            {
              SignAndSave (tracker, adef, assembliesToSave);
            }
          }
        }

        // If a referenced assembly changes this assembly's references, this assembly will be modified again. Mark unmodified before saving.
        assembliesToSave.Remove (assembly);
        tracker.MarkUnmodified (assembly);

        // Keep track of original name of this assembly before saving the module. The writer might change the name.
        var originalAssemblyName = assembly.Name.Clone();
        _writer.WriteModule (moduleDefinition);

        // If the writer has changed the name of this assembly, all assemblies referencing this assembly must be updated. Because of the recursive
        // call above, we can be sure that these assemblies will be saved after returning from this method: it is guaranteed that the referenced
        // assemblies are saved before the referencing assemblies.
        // The only case where this is not true is with circular references. In this case, the recursion will stop when the first assembly in the
        // cycle is reached again (because it has been marked unmodified before the recursive step, and the recursion will end when an unmodified
        // assembly is reached). In that case, it can be assumed that the other SignAndSave method will pick up the still-modified assemblies later.
        if (!originalAssemblyName.MatchesDefinition (assembly.Name))
        {
          foreach (var assemblyDefinition in tracker.GetReverseReferences (assembly))
          {
            UpdateReferences (assemblyDefinition, originalAssemblyName, assembly.Name);
            if (!assembliesToSave.Contains (assemblyDefinition))
            {
              assembliesToSave.Add (assemblyDefinition);
            }
          }
        }
      }
    }

    private void UpdateReferences (AssemblyDefinition assemblyDefinition, AssemblyNameReference oldDefinition, AssemblyNameDefinition newDefinition)
    {
      var nameReference = assemblyDefinition.Modules.SelectMany (m => m.AssemblyReferences)
          .Select ((reference, i) => new { Index = i, Reference = reference })
          .Single (tuple => tuple.Reference.MatchesDefinition (oldDefinition) || object.ReferenceEquals (tuple.Reference, newDefinition));
      
      assemblyDefinition.MainModule.AssemblyReferences[nameReference.Index].PublicKey = newDefinition.PublicKey;
      assemblyDefinition.MainModule.AssemblyReferences[nameReference.Index].HasPublicKey = newDefinition.HasPublicKey;
      assemblyDefinition.MainModule.AssemblyReferences[nameReference.Index].PublicKeyToken = newDefinition.PublicKeyToken;
      assemblyDefinition.MainModule.AssemblyReferences[nameReference.Index].Version = newDefinition.Version;
      assemblyDefinition.MainModule.AssemblyReferences[nameReference.Index].Hash = newDefinition.Hash;
      assemblyDefinition.MainModule.AssemblyReferences[nameReference.Index].HashAlgorithm = newDefinition.HashAlgorithm;
    }
  }
}