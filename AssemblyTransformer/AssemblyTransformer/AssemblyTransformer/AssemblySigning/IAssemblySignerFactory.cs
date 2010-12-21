// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using Mono.Options;

namespace AssemblyTransformer.AssemblySigning
{
  public interface IAssemblySignerFactory
  {
    void AddOptions (OptionSet options);
    IAssemblySigner CreateSigner ();
  }
}