using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace MarkVirtual
{
  /// <summary>
  /// data structure to manage assemblies which could be affected by marking methods virtual
  /// </summary>
  public class AssemblyManager : IEquatable<AssemblyManager>
  {
    private static ICollection<AssemblyManager> managers = new List<AssemblyManager>();

    public static IReferenceResolver resolver { get; set; }

    public static IVirtualMarker marker { get; set; }

    public static AssemblyManager GetAssemblyManager (AssemblyDefinition caller)
    {
      foreach (var mgr in managers)
      {
        if (mgr.Assembly.Name.FullName.Equals (caller.Name.FullName))
          return mgr;
      }
      var tmp = new AssemblyManager (caller, new List<AssemblyManager>(), new List<AssemblyManager>(), true, false);
      managers.Add (tmp);
      return tmp;
    }

    public string FullPath { get; set; }

    public bool IsValid { get; set; }

    public bool KeyChanged { get; set; }

    public AssemblyDefinition Assembly { get; set; }

    public ICollection<AssemblyManager> Dependencies { get; set; }

    public ICollection<AssemblyManager> References { get; set; }


    private AssemblyManager (
        AssemblyDefinition assembly,
        ICollection<AssemblyManager> dependencies,
        ICollection<AssemblyManager> references,
        bool modified,
        bool keyChanged)
    {
      Assembly = assembly;
      Dependencies = dependencies;
      IsValid = modified;
      References = references;
      IsValid = modified;
      KeyChanged = keyChanged;
    }

    public override string ToString ()
    {
      StringBuilder sb = new StringBuilder ("::: AssemblyManager: " + Assembly.Name.FullName);
      sb.AppendLine();
      sb.Append (":: Depends on:");
      sb.AppendLine();
      foreach (var def in Dependencies)
        sb.Append (def.ToStringDependencies ("  "));

      sb.Append (":: Referenced by:");
      sb.AppendLine();
      foreach (var def in References)
        sb.Append (def.ToStringReferences ("  "));
      return sb.ToString();
    }

    public string ToStringDependencies (string indent)
    {
      StringBuilder sb = new StringBuilder (indent);
      indent += "  ";
      sb.Append ("+-" + Assembly.Name);
      sb.AppendLine();
      foreach (var def in Dependencies)
        sb.Append (def.ToStringDependencies (indent));
      return sb.ToString();
    }

    public string ToStringReferences (string indent)
    {
      StringBuilder sb = new StringBuilder (indent);
      indent += "  ";
      sb.Append ("+-" + Assembly.Name);
      sb.AppendLine ();
      foreach (var def in References)
        sb.Append (def.ToStringReferences (indent));
      return sb.ToString ();
    }

    public bool ContainsDependency (String other)
    {
      return Dependencies.Select (dep => dep.Assembly.Name.FullName.Equals (other) | dep.ContainsDependency (other)).FirstOrDefault();
    }

    public bool ContainsReference (String other)
    {
      return References.Select (dep => dep.Assembly.Name.FullName.Equals (other) | dep.ContainsReference (other)).FirstOrDefault();
    }

    public bool ContainsDependency (AssemblyManager other)
    {
      return Dependencies.Contains (other);
    }

    public bool ContainsReference (AssemblyManager other)
    {
      return References.Contains (other);
    }

    public bool Equals (String other)
    {
      return this.Assembly.Name.Name.Equals (other);
    }

    public bool Equals (AssemblyManager other)
    {
      return this.Assembly.Name.FullName.Equals (other.Assembly.Name.FullName);
    }

    public new int GetHashCode ()
    {
      return this.Assembly.Name.FullName.GetHashCode();
    }
  }
}
