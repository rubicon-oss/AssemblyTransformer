using System;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblyMarking.MarkingStrategies
{
  public class NoneMarkingAttributeStrategy : IMarkingAttributeStrategy
  {
    public void AddCustomAttribute (MethodDefinition methodDefinition, ModuleDefinition moduleWithAttributeType)
    {
      // do nothing, since we dont want an attribute
    }
  }
}