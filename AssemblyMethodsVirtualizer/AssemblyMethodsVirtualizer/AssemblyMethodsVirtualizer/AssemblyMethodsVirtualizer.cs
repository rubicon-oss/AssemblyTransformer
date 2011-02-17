// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq;
using System.Text.RegularExpressions;
using AssemblyMethodsVirtualizer.MarkingStrategies;
using AssemblyTransformer;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;
using Mono.Cecil;

namespace AssemblyMethodsVirtualizer
{
  /// <summary>
  /// This is an implementation of an assembly transformation. 
  /// The transformation's goal is, to mark the targeted methods in the given assemblytracker's assemblies
  /// as virtual. Also the given marking strategy is executed on every target.
  /// The targets are: all methods in the given assembly whose full name matches the given regular expression.
  /// After a method has been set to virtual, the whole corresponding assembly is marked as modified for later
  /// handling.
  /// </summary>
  public class AssemblyMethodsVirtualizer : IAssemblyTransformation
  {
    private readonly IMarkingAttributeStrategy _markingAttributeStrategy;
    private readonly Regex _targetMethodsFullNameMatchingRegex;

    public IMarkingAttributeStrategy MarkingAttributeStrategy
    {
      get { return _markingAttributeStrategy; }
    }

    public Regex TargetMethodsFullNameMatchingRegex
    {
      get { return _targetMethodsFullNameMatchingRegex; }
    }

    public AssemblyMethodsVirtualizer (IMarkingAttributeStrategy markingAttributeStrategy, Regex targetMethodsFullNameMatchingRegex)
    {
      ArgumentUtility.CheckNotNull ("markingAttributeStrategy", markingAttributeStrategy);
      ArgumentUtility.CheckNotNull ("targetMethodsFullNameMatchingRegex", targetMethodsFullNameMatchingRegex);

      _markingAttributeStrategy = markingAttributeStrategy;
      _targetMethodsFullNameMatchingRegex = targetMethodsFullNameMatchingRegex;
    }

    public void Transform (IAssemblyTracker tracker)
    {
      ArgumentUtility.CheckNotNull ("tracker", tracker);

      var modifiedMethods = from assemblyDefinition in tracker.GetAssemblies ()
                            from moduleDefinition in assemblyDefinition.Modules
                            from typeDefinition in moduleDefinition.Types
                            from methodDefinition in typeDefinition.Methods
                            where _targetMethodsFullNameMatchingRegex.IsMatch (methodDefinition.FullName)
                            select new { Assembly = assemblyDefinition, Method = methodDefinition };

      foreach (var modifiedMethodDefinition in modifiedMethods.ToList())
      {
        tracker.MarkModified (modifiedMethodDefinition.Assembly);
        if (!modifiedMethodDefinition.Method.IsStatic && 
          !modifiedMethodDefinition.Method.IsConstructor && 
          !modifiedMethodDefinition.Method.IsFinal &&
          !modifiedMethodDefinition.Method.IsVirtual &&
          !modifiedMethodDefinition.Method.CustomAttributes.Any(ca => ca.AttributeType.Namespace == "System.Runtime.Serialization"))
        {
          //modifiedMethodDefinition.Method.IsVirtual = true;
          //modifiedMethodDefinition.Method.IsNewSlot = true;
          //_markingAttributeStrategy.AddCustomAttribute (modifiedMethodDefinition.Method, modifiedMethodDefinition.Assembly);
        }

      }
    }
  }
}