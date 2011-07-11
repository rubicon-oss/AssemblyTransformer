// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//

// activate to print information about performance and memory usage
// #define PERFORMANCE_TEST
// using System.Diagnostics;

using System;
using System.Collections.Generic;
using AssemblyTransformer.AppDomainBroker;
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
                      IAssemblySignerFactory signerFactory,
                      IAppDomainInfoBroker infoBroker)
    {
      ArgumentUtility.CheckNotNull ("trackerFactory", trackerFactory);
      ArgumentUtility.CheckNotNull ("transformationFactories", transformationFactories);
      ArgumentUtility.CheckNotNull ("signerFactory", signerFactory);

#if PERFORMANCE_TEST
      Stopwatch s = new Stopwatch ();
      Stopwatch total = new Stopwatch ();
      s.Start ();
      total.Start();
#endif

      var tracker = trackerFactory.CreateTracker();

#if PERFORMANCE_TEST
      total.Stop();
      Process procObj = Process.GetCurrentProcess ();
      Console.WriteLine (Environment.NewLine + "  Private Memory Size : {0:N0}" + Environment.NewLine +
      "  Virtual Memory Size : {1:N0}" + Environment.NewLine +
      "  Working Set Size: {2:N0}", procObj.PrivateMemorySize64, procObj.VirtualMemorySize64, procObj.WorkingSet64);
      Console.WriteLine (Environment.NewLine + "  Initialization:   " + s.Elapsed);
      Console.WriteLine (Environment.NewLine + "  press key to continue with transformations");
      total.Start ();
      s.Restart ();
#endif

      foreach (var factory in transformationFactories)
      {
        Console.WriteLine ("Transforming assemblies according to " + factory.GetType().Name);
        factory.CreateTransformation(infoBroker).Transform (tracker);
      }
      infoBroker.Unload();

#if PERFORMANCE_TEST
      total.Stop ();
      procObj = Process.GetCurrentProcess ();
      Console.WriteLine (Environment.NewLine + "  Private Memory Size : {0:N0}" + Environment.NewLine +
      "  Virtual Memory Size : {1:N0}" + Environment.NewLine +
      "  Working Set Size: {2:N0}", procObj.PrivateMemorySize64, procObj.VirtualMemorySize64, procObj.WorkingSet64);
      Console.WriteLine (Environment.NewLine + "  Transformation:   " + s.Elapsed);
      Console.WriteLine (Environment.NewLine + "  press key to continue with sign and save");
      total.Start ();
      s.Restart ();
#endif

      var signer = signerFactory.CreateSigner();
      Console.WriteLine ("Signing and writing the transformed assemblies ... ");
      signer.SignAndSave (tracker);

#if PERFORMANCE_TEST
      s.Stop();
      total.Stop ();
      procObj = Process.GetCurrentProcess ();
      Console.WriteLine (Environment.NewLine + "  Private Memory Size : {0:N0}" + Environment.NewLine +
      "  Virtual Memory Size : {1:N0}" + Environment.NewLine +
      "  Working Set Size: {2:N0}", procObj.PrivateMemorySize64, procObj.VirtualMemorySize64, procObj.WorkingSet64);
      Console.WriteLine (Environment.NewLine + "  Signing & Saving: " + s.Elapsed);
      Console.WriteLine (Environment.NewLine + "Total: " + total.Elapsed + Environment.NewLine);
#endif
    }
  }
}