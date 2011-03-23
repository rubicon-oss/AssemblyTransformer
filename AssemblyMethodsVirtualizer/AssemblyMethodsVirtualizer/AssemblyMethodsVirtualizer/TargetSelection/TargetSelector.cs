// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace AssemblyMethodsVirtualizer.TargetSelection
{
  public class TargetSelector : ITargetSelectionStrategy
  {
    private readonly List<ITargetSelectionStrategy> _strategies;

    public TargetSelector (List<ITargetSelectionStrategy> strategies)
    {
      _strategies = strategies;
    }

    public bool IsTarget (MethodDefinition method, AssemblyDefinition containingAssembly)
    {
      return _strategies.Any (strategy => strategy.IsTarget (method, containingAssembly));
    }
  }
}