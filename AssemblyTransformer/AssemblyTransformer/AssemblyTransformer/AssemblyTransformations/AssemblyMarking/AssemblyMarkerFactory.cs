// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Text.RegularExpressions;
using AssemblyTransformer.AssemblyTransformations.AssemblyMarking.MarkingStrategies;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;
using Mono.Options;

namespace AssemblyTransformer.AssemblyTransformations.AssemblyMarking
{
  public class AssemblyMarkerFactory : IAssemblyTransformationFactory
  {
    public enum AttributeMode { None, Custom, Default }

    private readonly IFileSystem _fileSystem;
    private Regex _regex;
    private AttributeMode _mode;
    private string _attName = "NonVirtualAttribute";
    private string _attNamespace = "NonVirtualAttribute";
    private string _attributeAssembly;

    public AssemblyMarkerFactory (IFileSystem fileSystem)
    {
      _fileSystem = fileSystem;
    }

    public void AddOptions (OptionSet options)
    {
      options.Add (
             "r|regex|methods=",
             "The regular expression matching the targeted methods.",
             r => _regex = new Regex (r));
      options.Add (
            "a|att|attribute=",         
            "Mark affected methods with Attribute [None | Default | Custom] (standard = None)",  
            att => _mode = (AttributeMode) Enum.Parse (typeof (AttributeMode), att));
      options.Add (
            "n|atNS|namespace=",        
            "The namespace of the attribute (default value: NonVirtualAttribute)",  
            ns => _attNamespace = ns);
      options.Add (
            "t|attType|attName=", 
            "The name of the attribute type (default value: NonVirtualAttribute)",  
            at => _attName = at);
      options.Add (
            "f|attFile|attributeFile=",
            "Customattribute to be used to mark methods. (dll or exe containing the type)",
            custAtt => _attributeAssembly = custAtt);
    }

    public IAssemblyTransformation CreateTransformation ()
    {
      if (_regex == null)
        throw new InvalidOperationException ("Initialize options first.");

      return new AssemblyMarker (CreateMarkingStrategy (_mode), _regex);
    }

    private IMarkingAttributeStrategy CreateMarkingStrategy (AttributeMode attributeMode)
    {
      switch (attributeMode)
      {
        case AttributeMode.Default:
          Console.WriteLine ("Using default attribute mechanism, generating attribute: " + _attNamespace + "." + _attName + " in main module.");
          return new DefaultMarkingAttributeStrategy (_attNamespace, _attName);

        case AttributeMode.Custom:
          ModuleDefinition customAttributeModule = null;
          try
          {
            foreach (var module in _fileSystem.ReadAssembly (_attributeAssembly).Modules)
            {
              foreach (var attType in module.Types)
              {
                if (attType.Namespace == _attNamespace && attType.Name == _attName)
                  customAttributeModule = module;
              }
            }

            if (customAttributeModule == null)
              throw new ArgumentException ("The given custom attribute is not available in the given assembly!");
          }
          catch (Exception e)
          {
            throw new ArgumentException ("The given custom attribute file could not be opened!");
          }
          Console.WriteLine ("Using the custom attribute: " + _attNamespace + "." + _attName + " in " + _attributeAssembly + ".");
          return new CustomMarkingAttributeStrategy (_attNamespace, _attName, customAttributeModule);

        default:
          Console.WriteLine ("Using no marking attribute!");
          return new NoneMarkingAttributeStrategy ();
      }
    }
  }
}