using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;
using System.Linq;
using AssemblyTransformer.AssemblyTransformations.AssemblyTransformationFactoryFactory;



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
    private static ICollection<IAssemblyTransformationFactory> _transformationFactories;
    private static AssemblySignerFactory _signerFactory;
    private static DirectoryBasedAssemblyTrackerFactory _trackerFactory;

    static int Main (string[] args)
    {

      InitializeTransformer (GetArgumentsFromFileOrCL (args));

      Console.WriteLine ("AssemblyTransformer starting up ...");
      try
      {
        var runner = new Runner();
        runner.Run (_trackerFactory, _transformationFactories, _signerFactory);
      }
      catch (ArgumentException e)
      {
        Console.WriteLine (e.Message);
        return -2;
      }

      Console.WriteLine ("Assemblies successfully loaded, transformed, signed and saved!");
      return 0;
    }

    private static IEnumerable<string> GetArgumentsFromFileOrCL (IEnumerable<string> args)
    {
      string optionsFile;
      if ((optionsFile = args.FirstOrDefault (a => a.StartsWith ("@"))) == null)
        return args;
      optionsFile = optionsFile.Remove (0, 1);
      if (!File.Exists (optionsFile))
        return args;

      var arguments = new List<string>();
      string line;
      using (var r = new StreamReader (optionsFile))
      {
        while ((line = r.ReadLine ()) != null)
          arguments.AddRange (line.Split (' '));
      }
      return arguments;
    }

    private static void InitializeTransformer (IEnumerable<string> args)
    {
      var showHelp = false;
      var optionSet = new OptionSet { { "h|?", "show help message and exit", v => showHelp = v != null } };
      var fileSystem = new FileSystem.FileSystem ();
      List<string> leftOver;

      // -- create all the transformations
      var transformationFactoryFactory = new DLLBasedTransformationFactoryFactory (fileSystem);
      transformationFactoryFactory.WorkingDirectory = ".";
      transformationFactoryFactory.AddOptions (optionSet);

      // -- add the assembly tracker   
      var trackerFactory = new DirectoryBasedAssemblyTrackerFactory (fileSystem);
      trackerFactory.AddOptions (optionSet);

      // -- add the assembly signer
      _signerFactory = new AssemblySignerFactory (fileSystem);
      _signerFactory.AddOptions (optionSet);

      // load the given transformations
      optionSet.Parse (args);

      _transformationFactories = transformationFactoryFactory.CreateTrackerFactories ();
      // -- add options of the transformations
      foreach (var factory in _transformationFactories)
        factory.AddOptions (optionSet);

      try
      {
        leftOver = optionSet.Parse (args);
        if (showHelp || leftOver.Count != 0)
          ShowHelp (optionSet, leftOver);
      }
      catch (OptionException e)
      {
        Console.WriteLine (e.Message);
        ShowHelp (optionSet, new string[0]);
      }
      _trackerFactory = trackerFactory;
    }

    private static void ShowHelp (OptionSet options, IEnumerable<string> leftOver)
    {
      Console.WriteLine ("#---------------------------------------------------------------------------------------#");
      if (leftOver.Count () != 0)
      {
        Console.WriteLine ("################# Wrong parameters! #################");
        foreach (var par in leftOver)
          Console.WriteLine (par);
      }
      Console.WriteLine ("\n  Usage:");
      Console.WriteLine ("  'AssemblyTransformer.exe ( [OPTIONS]+ | option file in this folder {'@filename.extension'} )' \n");
      Console.WriteLine ("  Reads all the Assemblies in this folder. (where this .exe lies) ");
      Console.WriteLine ("  The contained transformations are then instantiated and according to the ");
      Console.WriteLine ("  factories, the options are added and parsed. ");
      Console.WriteLine ("  All the Assemblies in the given target folder (including subfolders) ");
      Console.WriteLine ("  are loaded and passed to the transformations. ");
      Console.WriteLine ("  The assemblies will be resigned and saved after all transformations ");
      Console.WriteLine ("  have been conducted ");
      Console.WriteLine ("  Tries to use the correct keys (given in the [optional] keys folder), ");
      Console.WriteLine ("  but default behaviour is resigning all assemblies with [optional] given ");
      Console.WriteLine ("  default key. ");
      Console.WriteLine ("  If no key is given, the assemblies will be UNSIGNED!");
      Console.WriteLine ();
      Console.WriteLine ("  OPTIONS:\n");
      options.WriteOptionDescriptions (Console.Out);
      Environment.Exit (-2);
    }
  }
}
