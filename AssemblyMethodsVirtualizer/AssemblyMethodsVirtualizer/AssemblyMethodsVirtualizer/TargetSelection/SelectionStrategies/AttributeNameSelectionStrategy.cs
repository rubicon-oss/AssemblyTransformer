using System;
using System.Linq;
using Mono.Cecil;

namespace AssemblyMethodsVirtualizer.TargetSelection.SelectionStrategies
{
  class AttributeNameSelectionStrategy : IVisitorTargetSelectionStrategy
  {
    private readonly string _attributeName;

    public AttributeNameSelectionStrategy (string attributeName)
    {
      _attributeName = attributeName;
    }

    public bool IsTarget (MethodDefinition method)
    {
      return method.CustomAttributes.Any (att => att.AttributeType.FullName == _attributeName);
    }

    public bool AreAllMethodsOfTypeTarget (TypeDefinition typ)
    {
      return typ.CustomAttributes.Any (att => att.AttributeType.FullName == _attributeName);
    }
  }
}
