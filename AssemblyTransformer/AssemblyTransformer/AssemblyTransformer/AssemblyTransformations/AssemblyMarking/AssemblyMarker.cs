// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Text.RegularExpressions;
using System.Linq;
using AssemblyTransformer.AssemblyTransformations.AssemblyMarking.MarkingStrategies;

namespace AssemblyTransformer.AssemblyTransformations.AssemblyMarking
{
  // TODO Review FS: Consider renaming to MethodVirtualizer or something similar
  public class AssemblyMarker : IAssemblyTransformation
  {
    private readonly IMarkingAttributeStrategy _markingAttributeStrategy;
    private readonly Regex _regex;

    // TODO Review FS: Consider renaming regex and _regex to (_)virtualizedMethodFullNameRegex or something similar, so that it's clear that the regex is matched against method full names to decide which methods to make virtual
    public AssemblyMarker (IMarkingAttributeStrategy markingAttributeStrategy, Regex regex)
    {
      _markingAttributeStrategy = markingAttributeStrategy;
      _regex = regex;
    }

    public void Transform (IAssemblyTracker tracker)
    {
      var modifiedMethods = from assemblyDefinition in tracker.GetAssemblies ()
                            from moduleDefinition in assemblyDefinition.Modules
                            from typeDefinition in moduleDefinition.Types
                            from methodDefinition in typeDefinition.Methods
                            where _regex.IsMatch (methodDefinition.FullName)
                            select new { Assembly = assemblyDefinition, Method = methodDefinition };

      foreach (var modifiedMethodDefinition in modifiedMethods)
      {
        tracker.MarkModified (modifiedMethodDefinition.Assembly);
        
        modifiedMethodDefinition.Method.IsVirtual = true;
        _markingAttributeStrategy.AddCustomAttribute (modifiedMethodDefinition.Method, modifiedMethodDefinition.Assembly);
      }
    }
  }
}