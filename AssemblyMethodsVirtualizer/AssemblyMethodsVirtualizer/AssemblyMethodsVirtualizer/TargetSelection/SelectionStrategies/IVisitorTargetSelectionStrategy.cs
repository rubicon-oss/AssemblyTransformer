// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using Mono.Cecil;

namespace AssemblyMethodsVirtualizer.TargetSelection.SelectionStrategies
{
  public interface IVisitorTargetSelectionStrategy
  {
    // overrules IsTarget
    bool AreAllMethodsOfTypeTarget (TypeDefinition typ);

    // Is only called when AreAllMethodsOfTypeTarget is false
    bool IsTarget (MethodDefinition method);
  }
}