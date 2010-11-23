using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace MarkVirtual
{
  class Program
  {
    private static string workingDirectory = @"C:\temp\modify";
    private static string regexString = "(Car)(.*)[l|L]ock";

    static void Main (string[] args)
    {
      Console.WriteLine ("--- S T A R T ---\n");

      Arguments commandLine = new Arguments (args);

      if (commandLine["workingDir"] != null)
        workingDirectory = commandLine["workingDir"];
      if (commandLine["regex"] != null)
        regexString = commandLine["regex"];


      ModuleDefinition definition = ModuleDefinition.CreateModule ("attribute", ModuleKind.Dll);
      ReferenceResolver resolver = new ReferenceResolver (workingDirectory);
      StrongNameKeyPair defKey = new StrongNameKeyPair (File.Open ("key.snk", FileMode.Open));
      StrongNameKeyPair signKey = new StrongNameKeyPair (File.Open ("key2.snk", FileMode.Open));
      Marker marker = new Marker (
          new CustomAttribute (definition.Import (typeof (NonVirtualAttribute).GetConstructor (new Type[] { }))),
          signKey,
          new List<StrongNameKeyPair>() { /*defKey*/ });
      IModificationSerializer serializer = new Signer (
          signKey,
          new List<StrongNameKeyPair>() { /*defKey*/ });

      Console.WriteLine ();
      
      foreach (var assembly in resolver.AssemblyManagers)
      {
        //Console.WriteLine ("### orig: " + System.BitConverter.ToString (assembly.Assembly.Name.PublicKeyToken));
        
        //MemoryStream stream = new MemoryStream();
        //assembly.Assembly.Write (stream, new WriterParameters () { StrongNameKeyPair = signKey });
        //Console.WriteLine ("### orig written: " + System.BitConverter.ToString (assembly.Assembly.Name.PublicKeyToken));
        //stream.Position = 0;
        //AssemblyDefinition def = AssemblyDefinition.ReadAssembly (stream);
        //Console.WriteLine ("### new written: " + System.BitConverter.ToString (def.Name.PublicKeyToken));
        //Console.WriteLine ("### new: written: " + def.Name.Name);
        //Console.Write (assembly.IsValid + "  - ");
        Console.WriteLine (assembly);
        
        //Console.WriteLine (assembly.Assembly.MainModule.AssemblyReferences [assembly.Assembly.MainModule.AssemblyReferences.IndexOf (assembly.Assembly.Name)] = assembly.Assembly.Name);
      }
      //Console.WriteLine ();
      //Console.WriteLine (":::mark virtual:::");
      //Console.WriteLine (File.Exists ("key2.snk"));
      //Console.WriteLine (signKey.ToString());

      //foreach (var assembly in resolver.Assemblies)
      //{
      //  //if (assembly.Key.Name.Name.Equals ("Porsche"))
      //  //{
      //    //Console.WriteLine (assembly.Key.Name.FullName);
      //    MethodDefinition printLocked = assembly.Key.MainModule.Types[1].Methods[1];
      //    Regex match = new Regex ("(Car)(.*)[l|L]ock");
      //    if (match.IsMatch (printLocked.FullName))
      //    {
      //      Console.WriteLine (printLocked.FullName + " matches " + match.ToString() + " : " + match.IsMatch (printLocked.FullName));
      //      marker.MarkMethod (
      //          printLocked, new CustomAttribute (definition.Import (typeof (NonVirtualAttribute).GetConstructor (new Type[] { }))), true);
      //      AssemblyManager.GetAssemblyManager (assembly.Key).IsValid = false;
      //    }
      //  //marker.SerializeModifications (AssemblyManager.GetAssemblyManager (assembly.Key), signKey);
      //    //marker.Serialize (resolver.AssemblyManagers);
      //  //}
      //}

      marker.OverrideMethods (resolver.AssemblyManagers, new Regex (regexString), true);
      serializer.Serialize (resolver.AssemblyManagers);

      Console.WriteLine ("\n--- D O N E ---");
      Console.ReadLine ();
    }

  }
}
