// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Collections.Generic;
using AssemblyTransformer.AssemblyTracking;

namespace AssemblyTransformer.AssemblyTransformations.AssemblyTransformationFactoryFactory
{
  public interface ITransformationFactoryFactory
  {
    void AddOptions (OptionSet options);
    ICollection<IAssemblyTransformationFactory> CreateTrackerFactories ();
  }
}