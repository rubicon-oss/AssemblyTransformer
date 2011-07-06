using System;
using AssemblyTransformer;
using Mono.Cecil;

namespace AssemblyMethodsVirtualizer.MarkingStrategies
{
  /// <summary>
  /// The custom attribute strategy, allows the user to specify an attribute type contained in the given assembly.
  /// The strategy adds the needed reference to the module in case of an attribute usage.
  /// </summary>
  public class CustomMarkingAttributeStrategy : MarkingAttributeStrategy
  {
    private readonly ModuleDefinition _moduleContainingAttribute;
    private readonly string _unspeakablePrefix;

    public ModuleDefinition ModuleContainingAttribute
    {
      get { return _moduleContainingAttribute; }
    }

    public CustomMarkingAttributeStrategy (string attributeNameSpace, string attributeName, 
      ModuleDefinition moduleContainingAttribute, string unspeakablePrefix) 
            : base (attributeNameSpace, attributeName)
    {
      ArgumentUtility.CheckNotNull ("attributeNameSpace", attributeNameSpace);
      ArgumentUtility.CheckNotNull ("attributeName", attributeName);
      ArgumentUtility.CheckNotNull ("moduleContainingAttribute", moduleContainingAttribute);

      _unspeakablePrefix = unspeakablePrefix;
      _moduleContainingAttribute = moduleContainingAttribute;
    }

    public override void AddCustomAttribute (MethodDefinition methodDefinition, AssemblyDefinition assemblyOfMethod)
    {
      ArgumentUtility.CheckNotNull ("methodDefinition", methodDefinition);
      ArgumentUtility.CheckNotNull ("assemblyOfMethod", assemblyOfMethod);

      AddCustomAttribute (methodDefinition, _moduleContainingAttribute, 
        new CustomAttributeArgument(methodDefinition.Module.TypeSystem.String, _unspeakablePrefix + methodDefinition.Name)
        );
    }

    protected override TypeDefinition CreateCustomAttributeType (ModuleDefinition targetModule)
    {
      throw new InvalidOperationException ("The given attribute " + _attributeNamespace +"."+ _attributeName + " could not be found in the given assembly!");
    }
  }
}