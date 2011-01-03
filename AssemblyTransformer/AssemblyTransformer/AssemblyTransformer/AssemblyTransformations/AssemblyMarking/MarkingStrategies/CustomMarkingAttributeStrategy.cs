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
      // TODO Review FS: ArgumentException doesn't seem like the right exception type because it's not an argument to this method that is wrong. Use InvalidOperationException instead.
      throw new ArgumentException ("The given attribute " + _attributeNamespace +"."+ _attributeName + " could not be found in the given assembly!");
    }
  }
}