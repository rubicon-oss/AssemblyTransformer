using System;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblyTransformations.AssemblyMarking.MarkingStrategies
{
  public class CustomMarkingAttributeStrategy : MarkingAttributeStrategy
  {
    private readonly ModuleDefinition _moduleContainingAttribute;

    public CustomMarkingAttributeStrategy (string attributeNameSpace, string attributeName, ModuleDefinition moduleContainingAttribute) 
            : base (attributeNameSpace, attributeName)
    {
      _moduleContainingAttribute = moduleContainingAttribute;
    }

    public override void AddCustomAttribute (MethodDefinition methodDefinition, AssemblyDefinition assemblyOfMethod)
    {
      AddCustomAttribute (methodDefinition, _moduleContainingAttribute);
    }

    protected override TypeDefinition CreateCustomAttributeType (ModuleDefinition targetModule)
    {
      throw new ArgumentException ("The given Attribute " + _attributeNamespace +"."+ _attributeName + " could not be found in the given Assembly!");
    }
  }
}