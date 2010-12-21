// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using Mono.Options;

namespace AssemblyTransformer.AssemblyTracking
{
  public interface IAssemblyTrackerFactory
  {
    void AddOptions (OptionSet options);
    IAssemblyTracker CreateTracker ();
  }
}