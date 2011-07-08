using System;
using System.Collections.Generic;
using System.Reflection;

namespace AssemblyTransformer.AppDomainBroker
{
  public class AssemblyReferenceResolver : MarshalByRefObject, IDisposable
  {
    private readonly Dictionary<string, Assembly> _assemblies;

    public AssemblyReferenceResolver ()
    {
      _assemblies = new Dictionary<string, Assembly>();
      AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    public void Install (string codeBase)
    {
      var assembly = Assembly.LoadFrom (codeBase);
      _assemblies.Add (assembly.FullName, assembly);
    }

    public void Dispose ()
    {
      AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
    }

    Assembly CurrentDomain_AssemblyResolve (object sender, ResolveEventArgs args)
    {
      Assembly assembly;
      _assemblies.TryGetValue (args.Name, out assembly);
      return assembly;
    }
  }
}