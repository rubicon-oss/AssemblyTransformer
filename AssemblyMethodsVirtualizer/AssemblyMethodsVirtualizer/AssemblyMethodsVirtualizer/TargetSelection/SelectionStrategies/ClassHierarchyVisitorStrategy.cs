// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;

namespace AssemblyMethodsVirtualizer.TargetSelection.SelectionStrategies
{
  public class ClassHierarchyVisitorStrategy : ITargetSelectionStrategy
  {
    private readonly IList<IVisitorTargetSelectionStrategy> _visitorStrategies;
    private readonly IDictionary<Tuple<TypeDefinition, AssemblyDefinition>, bool> _targetTypes;
    private readonly IAssemblyTracker _tracker;

    public IDictionary<Tuple<TypeDefinition, AssemblyDefinition>, bool> TargetTypes
    {
      get { return _targetTypes; }
    }

    public ClassHierarchyVisitorStrategy (IAssemblyTracker tracker, IList<IVisitorTargetSelectionStrategy> visitorStrategies)
    {
      _visitorStrategies = visitorStrategies;
      _targetTypes = new Dictionary<Tuple<TypeDefinition, AssemblyDefinition>, bool>();
      _tracker = tracker;
      InitTargetTypes(_tracker);
    }

    private void InitTargetTypes (IAssemblyTracker tracker)
    {
      foreach (var assembly in tracker.GetAssemblies ())
        foreach (var module in assembly.Modules)
          foreach (var typ in module.Types)
            if (_visitorStrategies.Any (strategy => strategy.AreAllMethodsOfTypeTarget (typ)))
              _targetTypes.Add (Tuple.Create (typ, assembly), true);

      foreach (var assembly in tracker.GetAssemblies ())
        foreach (var module in assembly.Modules)
          foreach (var typ in module.Types)
            _targetTypes[Tuple.Create (typ, assembly)] = IsTargetRecursive (Tuple.Create (typ, assembly));
    }

    private bool IsTargetRecursive (Tuple<TypeDefinition, AssemblyDefinition> typeAndAssembly)
    {
      if (typeAndAssembly == null || typeAndAssembly.Item1.FullName == "<Module>" || typeAndAssembly.Item1.FullName == "System.Object")
        return false;

      if (!_targetTypes.ContainsKey (typeAndAssembly))
        _targetTypes[typeAndAssembly] = IsTargetRecursive (_tracker.TypeDefinitionCache[typeAndAssembly.Item1.BaseType, typeAndAssembly.Item2.Name]);

      return _targetTypes[typeAndAssembly];
    }


    public bool IsTarget (MethodDefinition method, AssemblyDefinition containingAssembly)
    {
      return  _targetTypes[_tracker.TypeDefinitionCache[method.DeclaringType, containingAssembly]] 
           || _visitorStrategies.Any (m => m.IsTarget (method));
    }
  }
}