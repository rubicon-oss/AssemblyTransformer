using Mono.Cecil;

namespace AssemblyTransformer.AssemblyTransformations.AssemblyMarking.MarkingStrategies
{
  public interface IMarkingAttributeStrategy
  {
    void AddCustomAttribute (MethodDefinition methodDefinition, AssemblyDefinition assemblyOfMethod);
  }
}