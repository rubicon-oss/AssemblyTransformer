// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace AssemblyTransformer.AssemblyMarking
{

  public class AssemblyMarker
  {
    private readonly IMarkingAttributeStrategy _markingAttributeStrategy;

    public AssemblyMarker (IMarkingAttributeStrategy markingAttributeStrategy)
    {
      _markingAttributeStrategy = markingAttributeStrategy;
    }

    public void OverrideMethods (IAssemblyTracker tracker, Regex regex)
    {
      var modifiedMethods = from assemblyDefinition in tracker.GetAssemblies ()
                            from moduleDefinition in assemblyDefinition.Modules
                            from typeDefinition in moduleDefinition.Types
                            from methodDefinition in typeDefinition.Methods
                            where regex.IsMatch (methodDefinition.FullName)
                            select new { Assembly = assemblyDefinition, Method = methodDefinition };

      foreach (var modifiedMethodDefinition in modifiedMethods)
      {
        tracker.MarkModified (modifiedMethodDefinition.Assembly);
        
        modifiedMethodDefinition.Method.IsVirtual = true;
        _markingAttributeStrategy.AddCustomAttribute (modifiedMethodDefinition.Method, modifiedMethodDefinition.Assembly.MainModule);
      }
    }
  }
}