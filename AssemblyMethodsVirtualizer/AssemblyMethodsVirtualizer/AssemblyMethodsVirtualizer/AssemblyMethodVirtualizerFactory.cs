// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyMethodsVirtualizer.ILCodeGeneration;
using AssemblyMethodsVirtualizer.MarkingStrategies;
using AssemblyMethodsVirtualizer.TargetSelection;
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
    private AttributeMode _mode;
    private string _attName = "NonVirtualAttribute";
    private string _attNamespace = "NonVirtualAttribute";
    private string _unspeakablePrefix = "<>virtualized_";
    private string _attributeAssembly;
    private ITargetSelectionFactory _selectionFactory;

    public AssemblyMethodVirtualizerFactory (IFileSystem fileSystem, string workingDirectory)
    {
      ArgumentUtility.CheckNotNull ("fileSystem", fileSystem);

      _fileSystem = fileSystem;
    }

    public void AddOptions (OptionSet options)
    {
      ArgumentUtility.CheckNotNull ("options", options);

      options.Add (
            "att|attribute=",         
            "Mark affected methods with custom attribute [None | Generated | Custom] (default = Generated)",  
            att => _mode = (AttributeMode) Enum.Parse (typeof (AttributeMode), att));
      options.Add (
            "attPrefix=",
            "The unspeakable prefix for the virtual method. (default value: '<>virtualized_')",
            prefix => _unspeakablePrefix = prefix);
      options.Add (
            "attFullName=",
            "Fullname of the attribute type (default value: 'NonVirtualAttributeNonVirtualAttribute').",
              at => {
                _attName = at.Substring (at.LastIndexOf (".") + 1, at.Length - at.LastIndexOf (".") - 1);
                _attNamespace = at.Substring (0, at.LastIndexOf ("."));
              } );
      options.Add (
            "attFile|attributeFile=",
            "Assembly containing the custom attribute (dll or exe). ONLY applicable in 'Custom' attribute mode!", 
            custAtt => _attributeAssembly = custAtt);

      _selectionFactory = new TargetSelectorFactory ();
      _selectionFactory.AddOptions (options);
    }

    public IAssemblyTransformation CreateTransformation ()
    {
      if (_selectionFactory == null)
        throw new InvalidOperationException("Initialize options first! (AssemblyMethodVirtualizer)");
      return new AssemblyMethodsVirtualizer (CreateMarkingStrategy (_mode), _selectionFactory, new ILCodeGenerator(_unspeakablePrefix));
    }

    private IMarkingAttributeStrategy CreateMarkingStrategy (AttributeMode attributeMode)
    {
      switch (attributeMode)
      {
        case AttributeMode.None:
          Console.WriteLine ("MethodVirtualizer: Using no marking attribute!");
          return new NoneMarkingAttributeStrategy ();

        case AttributeMode.Custom:
          ModuleDefinition customAttributeModule = null;
          try
          {
            foreach (var module in _fileSystem.ReadAssembly (_attributeAssembly, new ReaderParameters {ReadSymbols = false}).Modules)
            {
              foreach (var attType in module.Types)
              {
                if (attType.Namespace == _attNamespace && attType.Name == _attName)
                  customAttributeModule = module;
              }
            }
            if (customAttributeModule == null)
              throw new ProgramArgumentException ("MethodVirtualizer: The given custom attribute is not available in the given assembly!");
          }
          catch (BadImageFormatException e)
          {
            throw new ArgumentException ("MethodVirtualizer: The given custom attribute file could not be opened!", e);
          }
          Console.WriteLine ("MethodVirtualizer: Using the custom attribute: " + _attNamespace + "." + _attName + " in " + _attributeAssembly + ".");
          return new CustomMarkingAttributeStrategy (_attNamespace, _attName, customAttributeModule, _unspeakablePrefix);

        default:
          Console.WriteLine ("MethodVirtualizer: Using default attribute mechanism, generating attribute: " + _attNamespace + "." + _attName + " in main module.");
          return new GeneratedMarkingAttributeStrategy (_attNamespace, _attName);
          
      }
    }
  }
}