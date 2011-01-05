// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;

namespace AssemblyTransformer.AssemblySigning
{
  /// <summary>
  /// The assembly signer factory looks the same as the transformation and tracker factory, but is seperated, because
  /// there can/should be only ONE signer at the end of the process. The developer has to pass the assemblies to multiple
  /// signers himself!
  /// options have to be added before signer is created.
  /// </summary>
  public interface IAssemblySignerFactory
  {
    void AddOptions (OptionSet options);
    IAssemblySigner CreateSigner ();
  }
}