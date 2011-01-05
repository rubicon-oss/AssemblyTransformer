// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;

namespace AssemblyTransformer.AssemblyTracking
{
  /// <summary>
  /// The assembly tracker has to be instantiated by a factory. This looks the same like the transformations factory, but
  /// it is seperated, because only ONE assembly tracker can/should be used at the beginning of the transformation process.
  /// If multiple trackers are used, the different assemblytrackers have to be passed to the transformations by the developer!
  /// AddOptions has to be called before the instantiation of the tracker, otherwise the parameters will be ommited.
  /// </summary>
  public interface IAssemblyTrackerFactory
  {
    void AddOptions (OptionSet options);
    IAssemblyTracker CreateTracker ();
  }
}