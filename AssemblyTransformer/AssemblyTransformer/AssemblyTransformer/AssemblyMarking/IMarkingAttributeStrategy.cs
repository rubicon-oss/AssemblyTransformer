using Mono.Cecil;

namespace AssemblyTransformer.AssemblyMarking
{
  public interface IMarkingAttributeStrategy
  {
    void AddCustomAttribute (MethodDefinition methodDefinition, ModuleDefinition moduleWithAttributeType);
  }
}