using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace MarkVirtual
{
  internal class ReferenceResolver : IReferenceResolver
  {
    #region Members

    private IDictionary<AssemblyDefinition, ICollection<AssemblyDefinition>> Assemblies { get; set; }

    //private ICollection<AssemblyDefinition> Dependencies { get; set; }
    //private IDictionary<AssemblyManager, ICollection<AssemblyManager>> ReverseReferences { get; set; }


    public string WorkingDir { get; set; }

    public ICollection<AssemblyManager> AssemblyManagers { get; private set; }

    #endregion

    public static ICollection<AssemblyManager> GetAssembliesInMemory (string targetDirectory)
    {
      return new ReferenceResolver (targetDirectory).AssemblyManagers;
    }

    private ReferenceResolver ()
    {}

    public ReferenceResolver (string workingDir)
    {
      if ( workingDir == null )
        WorkingDir = Directory.GetCurrentDirectory ();
      else
        WorkingDir = workingDir;
      Init ();
    }

    private void Init ()
    {
      Console.WriteLine ("Initializing... ");
      Assemblies = new Dictionary<AssemblyDefinition, ICollection<AssemblyDefinition>>();
      //Dependencies = new List<AssemblyDefinition>();
      AssemblyManagers = new List<AssemblyManager>();
      //ReverseReferences = new Dictionary<AssemblyManager, ICollection<AssemblyManager>>();

      foreach (var doc in Directory.EnumerateFiles (WorkingDir, "*.dll", SearchOption.AllDirectories))
      {
        Console.WriteLine ("  Processing " + doc + " ...");
        try
        {
            AssemblyDefinition tmpDef = AssemblyDefinition.ReadAssembly (doc);
            Assemblies.Add (tmpDef, new List<AssemblyDefinition>());
            AssemblyManager tmpMgr = AssemblyManager.GetAssemblyManager (tmpDef);
            tmpMgr.FullPath = doc;
            AssemblyManagers.Add (tmpMgr);
            FindAssemblyDependencies (tmpMgr);
        }
        catch (Exception)
        {
          Console.WriteLine (" ! not a .NET assembly [" + doc + "] !");
        }
      }
      Console.WriteLine ("  Resolve reverse references... ");
      foreach (var mgr in AssemblyManagers)
      {
        //FindDependentAssemblies (mgr, new List<AssemblyManager>());
        //mgr.References = ReverseReferences[mgr];
        FindDependentAssemblies (mgr);
      }
      Console.WriteLine ("Successfully initialized!\n");
    }

    private AssemblyDefinition GetReference (string p)
    {
      return Assemblies.Keys.FirstOrDefault (assembly => assembly.Name.FullName.Equals (p));
    }

    //private void ResolveDependencies(ICollection<AssemblyDefinition> assm)
    //{
    //    if (assm == null)
    //    {
    //        Console.WriteLine("ResolveDependencies: null");
    //        return;
    //    }
    //    AssemblyDefinition tmpAsm;
    //    for (int i = 0; i < Dependencies.Count; ++i)
    //    {
    //        foreach (var assmreference in assm.ElementAt(i).Modules)
    //        {
    //            foreach (var namereference in assmreference.AssemblyReferences)
    //            {
    //                tmpAsm = GetReference(namereference.FullName);
    //                if (tmpAsm != null && !assm.Contains(tmpAsm))
    //                {
    //                    assm.Add(tmpAsm);
    //                }
    //            }
    //        }
    //    }
    //}

    //private AssemblyManager FindAssemblyDependencies (AssemblyDefinition caller)
    //{
    //  AssemblyManager mgr = AssemblyManager.GetAssemblyManager (caller);
    //  AssemblyDefinition tmpAsm;
    //  foreach (var assmreference in mgr.Assembly.Modules)
    //  {
    //    foreach (var namereference in assmreference.AssemblyReferences)
    //    {
    //      tmpAsm = GetReference (namereference.FullName);
    //      if (tmpAsm != null && !mgr.ContainsDependency (namereference.FullName))
    //        mgr.Dependencies.Add (FindAssemblyDependencies (tmpAsm));
    //    }
    //  }
    //  return mgr;
    //}

    private AssemblyManager FindAssemblyDependencies (AssemblyManager caller)
    {
      //AssemblyManager mgr = AssemblyManager.GetAssemblyManager (caller);
      AssemblyDefinition tmpAsm;
      foreach (var assmreference in caller.Assembly.Modules)
      {
        foreach (var namereference in assmreference.AssemblyReferences)
        {
          tmpAsm = GetReference (namereference.FullName);
          if (tmpAsm != null && !caller.ContainsDependency (namereference.FullName))
            caller.Dependencies.Add (FindAssemblyDependencies (AssemblyManager.GetAssemblyManager (tmpAsm)) );
        }
      }
      return caller;
    }

    //private void FindDependentAssemblies (AssemblyManager assm, ICollection<AssemblyManager> references)
    //{
    //  if (!ReverseReferences.ContainsKey (assm))
    //    ReverseReferences.Add (assm, references);
    //  foreach (var mgr in references)
    //  {
    //    if (!ReverseReferences[assm].Contains (mgr))
    //      ReverseReferences[assm].Add (mgr);
    //  }
    //  List<AssemblyManager> tmpColl = new List<AssemblyManager> (references) { assm };
    //  foreach (var mgr in assm.Dependencies)
    //    FindDependentAssemblies (mgr, tmpColl);
    //}

    private void FindDependentAssemblies (AssemblyManager assm)
    {
      foreach (var manager in assm.Dependencies)
      {
        if (! manager.References.Contains(assm))
          manager.References.Add (assm);
        FindDependentAssemblies (manager);
      }
    }

  }
}
