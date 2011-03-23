// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using AssemblyTransformer;
using AssemblyTransformer.AssemblyTracking;

namespace AssemblyMethodsVirtualizer.TargetSelection
{
  public interface ITargetSelectionFactory
  {
    void AddOptions (OptionSet options);
    ITargetSelectionStrategy CreateSelector (IAssemblyTracker tracker);
  }
}