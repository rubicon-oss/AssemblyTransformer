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
  /// <summary>
  /// The runner instantiates the transformations, one by one and executes them in a serial way.
  /// Before the transformations, the tracker has to be instantiated and initialized, because the transformations
  /// need the assemblies. After the transformations have taken place, the signer is instantiated and used to
  /// sign and save the assemblies.
  /// </summary>
  public class Runner
  {
    public void Run (IAssemblyTrackerFactory trackerFactory,
                      IEnumerable<IAssemblyTransformationFactory> transformationFactories, 
                      IAssemblySignerFactory signerFactory)
    {
      ArgumentUtility.CheckNotNull ("trackerFactory", trackerFactory);
      ArgumentUtility.CheckNotNull ("transformationFactories", transformationFactories);
      ArgumentUtility.CheckNotNull ("signerFactory", signerFactory);

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