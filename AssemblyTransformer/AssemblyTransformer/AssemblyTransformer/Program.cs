﻿using System;
using System.Collections.Generic;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;
using AssemblyTransformer.AssemblyTransformations.AssemblyMethodsVirtualizing;

namespace AssemblyTransformer
{
  /// <summary>
  /// The main program of the assemblytransformer is responsible for adding the essential objects ( FileSystem, AssemblyTracker, AssemblySigner)
  /// to the transformation process. also the assembly transformations have to be added in order to be executed.
  /// This program also reads the parameters from the commandline and takes care of diverse argument exceptions (which means, print it to the user
  /// and exit).
  /// </summary>
  class Program
  {
    static int Main (string[] args)
    {
      var showHelp = false;
      var optionSet = new OptionSet { { "h|?|help", "show help message and exit", v => showHelp = v != null } };
      var fileSystem = new FileSystem.FileSystem ();
      var transformationFactories = new List<IAssemblyTransformationFactory> ();

 // -- add the assembly tracker   
      var trackerFactory = new DirectoryBasedAssemblyTrackerFactory(fileSystem);
      trackerFactory.AddOptions (optionSet);

 // -- add all the assembly transformations
      transformationFactories.Add (new AssemblyMethodVirtualizerFactory (fileSystem));
      foreach (var factory in transformationFactories)
        factory.AddOptions (optionSet);

 // -- add the assembly signer
      var signerFactory = new AssemblySignerFactory (fileSystem);
      signerFactory.AddOptions (optionSet);

      try
      {
        optionSet.Parse (args);
        if (showHelp)
        {
          ShowHelp (optionSet);
          return -2;
        }
      }
      catch (OptionException e)
      {
        Console.WriteLine (e.Message);
        ShowHelp (optionSet);
        return -2;
      }

      Console.WriteLine ("AssemblyTransformer starting up ...");
      try
      {
        var runner = new Runner();
        runner.Run (trackerFactory, transformationFactories, signerFactory);
      }
      catch (ArgumentException e)
      {
        Console.WriteLine (e.Message);
        return -2;
      }

      Console.WriteLine ("Assemblies successfully loaded, transformed, signed and saved!");
      return 0;
    }

    private static void ShowHelp (OptionSet options)
    {
      Console.WriteLine ("Usage: AssemblyTransformer.exe [OPTIONS]+");
      Console.WriteLine ("Marks all the Methods in the Assemblies existing in ");
      Console.WriteLine ("the given root folder (includes subfolders), that ");
      Console.WriteLine ("match the given regular expression as virtual. ");
      Console.WriteLine ("Resigns the assemblies afterwards and resets the references. ");
      Console.WriteLine ("Tries to use the correct keys ");
      Console.WriteLine ("(given in the optional keys folder) but default behaviour ");
      Console.WriteLine ("is resigning all with given default key. If no key is given, ");
      Console.WriteLine ("the assemblies are unsigned!");
      Console.WriteLine ();
      Console.WriteLine ("Options:");
      options.WriteOptionDescriptions (Console.Out);
      return;
    }
  }
}
