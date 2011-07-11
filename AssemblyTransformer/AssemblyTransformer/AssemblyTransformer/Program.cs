using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AssemblyTransformer.AppDomainBroker;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;
using System.Linq;
using AssemblyTransformer.AssemblyTransformations.AssemblyTransformationFactoryFactory;
using Mono.Cecil;


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
    private static IAppDomainInfoBroker _infoBroker;
    private static string _workingDirectory = ".";
    private static List<string> _targetFiles;

    static int Main (string[] args)
    {
      var arguments = GetArgumentsFromFileOrCL (args);
      InitializeTransformer (arguments);

      Console.WriteLine ("\nAssemblyTransformer starting up ...");
      try
      {
        var runner = new Runner();
        runner.Run (_trackerFactory, _transformationFactories, _signerFactory, _infoBroker);
      }
      catch (ArgumentException e)
      {
        Console.WriteLine ("\n" + e.Message + "\n");
        InitializeTransformer (arguments.Concat (new [] {"-h"}));
        return -2;
      }
      catch (InvalidOperationException e)
      {
        Console.WriteLine ("\n" + e.Message + "\n");
        InitializeTransformer (arguments.Concat (new[] { "-h" }));
        return -2;
      }

      if (_targetFiles.Count < 1)
        return 0;

      Console.Write ("Processed: \n" + _targetFiles[0]);

      for (int i = 1; i < _targetFiles.Count; i++)
      {
        Console.Write (_targetFiles[i] + "   ");
        if (i % 3 == 0)
          Console.WriteLine ();
      }
      Console.WriteLine ();

      return 0;
    }

    private static IEnumerable<string> GetArgumentsFromFileOrCL (IEnumerable<string> args)
    {
      if (args == null || args.Count() == 0)
        return new[] { "-h" }; // if no arguments given, set help parameter to show help in CL

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
        {
          if (line.Length != 0 && !line.StartsWith ("#"))
            arguments.Add (line);
        }
      }
      return arguments;
    }

    private static void InitializeTransformer (IEnumerable<string> args)
    {
      var showHelp = false;
      var globalOptions = new OptionSet { { "h|?", "Show this help message and exit.", v => showHelp = v != null } };
      globalOptions.Add ("d|targetDir=", "The (root) directory containing the targeted assemblies.", dir => _workingDirectory = dir);

      var fileSystem = new FileSystem.FileSystem();
      Dictionary<string, OptionSet> options = new Dictionary<string, OptionSet>();
      globalOptions.Parse (args);

      // -- create AppDomainInfoBroker
      _workingDirectory = _workingDirectory.Replace ("\"", "");
      _workingDirectory = _workingDirectory.TrimEnd (new[] { '\\' });
      _infoBroker = new AppDomainInfoBroker (_workingDirectory);

      ((DefaultAssemblyResolver) GlobalAssemblyResolver.Instance).RemoveSearchDirectory (".");
      ((DefaultAssemblyResolver) GlobalAssemblyResolver.Instance).RemoveSearchDirectory ("bin");
      ((DefaultAssemblyResolver) GlobalAssemblyResolver.Instance).AddSearchDirectory (_workingDirectory);

      // -- create all the transformations
      var transformationFactoryFactory = new DLLBasedTransformationFactoryFactory (fileSystem, _workingDirectory);
      transformationFactoryFactory.TransformationsDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      transformationFactoryFactory.AddOptions (globalOptions);

      // -- add the assembly tracker   
      var trackerFactory = new DirectoryBasedAssemblyTrackerFactory (fileSystem, _workingDirectory);
      trackerFactory.AddOptions (globalOptions);

      // -- add the assembly signer
      _signerFactory = new AssemblySignerFactory (fileSystem);
      _signerFactory.AddOptions (globalOptions);

      // load the given transformations
      globalOptions.Parse (args);

      _transformationFactories = transformationFactoryFactory.CreateTrackerFactories ();
      // -- add options of the transformations
      foreach (var factory in _transformationFactories)
      {
        var tempOptions = new OptionSet();
        factory.AddOptions (tempOptions);
        options[factory.GetType().Name] = tempOptions;
      }

      try
      {
        var allOptions = new OptionSet();
        foreach (var option in globalOptions)
          allOptions.Add (option);
        foreach (var set in options.Values)
          foreach (var option in set)
            allOptions.Add (option);

        var leftOver = allOptions.Parse (args);
        trackerFactory.IncludeFiles = leftOver.Where (s => (!s.StartsWith ("-") || !s.StartsWith ("\\"))).ToList();
        _targetFiles = trackerFactory.IncludeFiles;
        leftOver.RemoveAll (s => (!s.StartsWith ("-") || !s.StartsWith ("\\")));

        if (showHelp || leftOver.Count != 0)
          ShowHelp (globalOptions, options, leftOver);
      }
      catch (OptionException e)
      {
        Console.WriteLine (e.Message);
        ShowHelp (globalOptions, options, new string[0]);
      }
      _trackerFactory = trackerFactory;
    }

    private static void ShowHelp (OptionSet globalOptions, Dictionary<string, OptionSet> options, IEnumerable<string> leftOver)
    {
      Console.WriteLine ("#---------------------------------------------------------------------------------------#");
      if (leftOver.Count () != 0)
      {
        Console.WriteLine ("\n#####################################################");
        Console.WriteLine ("################# Wrong parameters! #################\n");
        foreach (var par in leftOver)
          Console.WriteLine (par);
      }
      Console.WriteLine ("\n\n  USAGE:");
      Console.WriteLine ("  'AssemblyTransformer.exe ( [OPTIONS]+ | option file {'@filename.extension'} )' \n");
      Console.WriteLine ("  Loads the transformations that are given using the '-t' switch ");
      Console.WriteLine ("  The transformations are then instantiated and according to the ");
      Console.WriteLine ("  factories, the options are added and parsed. ");
      Console.WriteLine ("  The target assemblies have to be pased as unnamed arguments. ");
      Console.WriteLine ("  The assemblies will be resigned and saved after all transformations ");
      Console.WriteLine ("  have been conducted. ");
      Console.WriteLine ("  Tries to use the correct keys (given in the [optional] keys folder), ");
      Console.WriteLine ("  but the default behaviour is resigning all assemblies with the [optional] given ");
      Console.WriteLine ("  default key. ");
      Console.WriteLine ("  If no key is given, the assemblies will be UNSIGNED!");
      Console.WriteLine ();
      Console.WriteLine ("  OPTIONS:\n");
      Console.WriteLine ("  # Global Options:\n");
      globalOptions.WriteOptionDescriptions (Console.Out);
      foreach (var option in options)
      {
        Console.WriteLine ("\n  # " + option.Key + "\n");
        option.Value.WriteOptionDescriptions (Console.Out);
      }
      Environment.Exit (-2);
    }
  }
}
