// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Text.RegularExpressions;
using AssemblyMethodsVirtualizer.MarkingStrategies;
using AssemblyTransformer;
using AssemblyTransformer.AssemblyTransformations;
using AssemblyTransformer.FileSystem;
using Mono.Cecil;

namespace AssemblyMethodsVirtualizer
{
  /// <summary>
  /// This factory is responsible for the correct instantiation of the assembly method virtualizer transformation.
  /// All the needed parameters are added to the options and parsed for the instantiation of the correct marking
  /// strategy. 
  /// </summary>
  public class AssemblyMethodVirtualizerFactory : IAssemblyTransformationFactory
  {
    public enum AttributeMode { None, Custom, Generated }

    private readonly IFileSystem _fileSystem;
    private Regex _regex;
    private AttributeMode _mode;
    private string _attName = "NonVirtualAttribute";
    private string _attNamespace = "NonVirtualAttribute";
    private string _attributeAssembly;

    public AssemblyMethodVirtualizerFactory (IFileSystem fileSystem)
    {
      ArgumentUtility.CheckNotNull ("fileSystem", fileSystem);

      _fileSystem = fileSystem;
    }

    public void AddOptions (OptionSet options)
    {
      ArgumentUtility.CheckNotNull ("options", options);

      options.Add (
             "r|regex|methods=",
             "The regular expression matching the targeted methods full name.",
             r => _regex = new Regex (r));
      options.Add (
            "a|att|attribute=",         
            "Mark affected methods with Attribute [None | Generated | Custom] (standard = None)",  
            att => _mode = (AttributeMode) Enum.Parse (typeof (AttributeMode), att));
      options.Add (
            "n|atNS|namespace=",
            "The namespace of the attribute (default value: NonVirtualAttribute). This is used for both the Generated and Custom attribute!",  
            ns => _attNamespace = ns);
      options.Add (
            "t|attType|attName=",
            "The name of the attribute type (default value: NonVirtualAttribute). This is used for both the Generated and Custom attribute!",  
            at => _attName = at);
      options.Add (
            "f|attFile|attributeFile=",
            "Custom attribute to be used to mark methods (dll or exe containing the type). ONLY applicable on Custom attribute mode!", 
            custAtt => _attributeAssembly = custAtt);
    }

    public IAssemblyTransformation CreateTransformation ()
    {
      if (_regex == null || _mode == null)
        throw new InvalidOperationException ("Initialize options first.");

      return new AssemblyMethodsVirtualizer (CreateMarkingStrategy (_mode), _regex);
    }

    private IMarkingAttributeStrategy CreateMarkingStrategy (AttributeMode attributeMode)
    {
      switch (attributeMode)
      {
        case AttributeMode.Generated:
          Console.WriteLine ("Using default attribute mechanism, generating attribute: " + _attNamespace + "." + _attName + " in main module.");
          return new GeneratedMarkingAttributeStrategy (_attNamespace, _attName);

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
              throw new ProgramArgumentException ("The given custom attribute is not available in the given assembly!");
          }
          catch (BadImageFormatException e)
          {
            throw new ArgumentException ("The given custom attribute file could not be opened!", e);
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