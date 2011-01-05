// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer.AssemblyTracking;

namespace AssemblyTransformer.AssemblySigning
{
  /// <summary>
  /// An assembly signer provides the core functionality of signing and saving a whole "assembly tree". Which means,
  /// all the dependencies and references have to be taken care of!
  /// </summary>
  public interface IAssemblySigner
  {
    void SignAndSave (IAssemblyTracker tracker);
  }
}