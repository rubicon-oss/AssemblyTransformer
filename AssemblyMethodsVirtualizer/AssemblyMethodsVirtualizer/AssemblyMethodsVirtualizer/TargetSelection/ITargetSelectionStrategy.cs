// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using AssemblyMethodsVirtualizer.TargetSelection.SelectionStrategies;
using Mono.Cecil;

namespace AssemblyMethodsVirtualizer.TargetSelection
{
  public interface ITargetSelectionStrategy
  {
    bool IsTarget (MethodDefinition method, AssemblyDefinition containingAssembly);
  }
}