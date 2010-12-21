// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using Mono.Options;

namespace AssemblyTransformer.AssemblyTransformations
{
  public interface IAssemblyTransformationFactory
  {
    void AddOptions (OptionSet options);
    IAssemblyTransformation CreateTransformation ();
  }
}