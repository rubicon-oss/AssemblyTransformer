using Mono.Cecil;

namespace AssemblyTransformer.AssemblyTransformations.AssemblyMethodsVirtualizing.MarkingStrategies
{
  /// <summary>
  /// This is the simplest of all marking strategies. No attribute or anything else is used to mark the 
  /// modified methods!
  /// </summary>
  public class NoneMarkingAttributeStrategy : IMarkingAttributeStrategy
  {
    public void AddCustomAttribute (MethodDefinition methodDefinition, AssemblyDefinition moduleWithAttributeType)
    {
      // do nothing, since we dont want an attribute
    }
  }
}