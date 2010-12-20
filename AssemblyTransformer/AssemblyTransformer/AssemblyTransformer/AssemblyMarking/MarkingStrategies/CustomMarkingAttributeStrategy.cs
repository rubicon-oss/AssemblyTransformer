using System;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblyMarking.MarkingStrategies
{
  public class CustomMarkingAttributeStrategy : MarkingAttributeStrategy
  {

    public CustomMarkingAttributeStrategy (string attributeNameSpace, string attributeName) 
            : base (attributeNameSpace, attributeName)
    {}

    protected override TypeDefinition CreateCustomAttributeType (ModuleDefinition targetModule)
    {
      throw new ArgumentException ("The given Attribute " + _attributeNamespace +"."+ _attributeName + " could not be found in the given Assembly!");
    }
  }
}