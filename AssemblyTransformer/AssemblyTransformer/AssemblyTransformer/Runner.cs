// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;

namespace AssemblyTransformer
{
  // TODO Review FS: Consider adding a unit test for the Run method
  public class Runner
  {
    public void Run (DirectoryBasedAssemblyTrackerFactory trackerFactory,
                      IEnumerable<IAssemblyTransformationFactory> transformationFactories, 
                      AssemblySignerFactory signerFactory)
    {
      var tracker = trackerFactory.CreateTracker();

      foreach (var factory in transformationFactories)
      {
        Console.WriteLine ("Transforming assemblies according to " + factory.GetType().Name);
        factory.CreateTransformation().Transform (tracker);
      }

      var signer = signerFactory.CreateSigner();
      Console.WriteLine ("Signing and writing the transformed assemblies ... ");
      signer.SignAndSave (tracker);
    }
  }
}