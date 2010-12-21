using Mono.Cecil;

namespace AssemblyTransformer.AssemblyTransformations.AssemblyMarking.MarkingStrategies
{
  public class NoneMarkingAttributeStrategy : IMarkingAttributeStrategy
  {
    public void AddCustomAttribute (MethodDefinition methodDefinition, AssemblyDefinition moduleWithAttributeType)
    {
      // do nothing, since we dont want an attribute
    }
  }
}