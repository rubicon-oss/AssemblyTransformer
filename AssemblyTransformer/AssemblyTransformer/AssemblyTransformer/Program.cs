using System;
using System.Collections.Generic;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblySigning.AssemblyWriting;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;
using AssemblyTransformer.AssemblyTransformations.AssemblyMarking;
using AssemblyTransformer.AssemblyTransformations.AssemblyMarking.MarkingStrategies;
using Mono.Cecil;
using Mono.Options;

namespace AssemblyTransformer
{
  class Program
  {
    static void Main (string[] args)
    {
      var showHelp = false;
      var optionSet = new OptionSet { { "h|?|help", "show help message and exit", v => showHelp = v != null } };
      var fileSystem = new FileSystem.FileSystem ();
      var transformationFactories = new List<IAssemblyTransformationFactory> ();
      var trackerFactory = new DirectoryBasedAssemblyTrackerFactory(fileSystem);
      trackerFactory.AddOptions (optionSet);
      
      transformationFactories.Add (new AssemblyMarkerFactory (fileSystem));
      foreach (var factory in transformationFactories)
        factory.AddOptions (optionSet);

      var signerFactory = new AssemblySignerFactory (fileSystem);
      signerFactory.AddOptions (optionSet);

      try
      {
        optionSet.Parse (args);
        if (showHelp)
          ShowHelp (optionSet);
      }
      catch (OptionException e)
      {
        Console.WriteLine (e.Message);
        ShowHelp (optionSet);
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
        return;
      }

      Console.WriteLine ("Assemblies successfully loaded, transformed, signed and saved!");
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
