using Mono.Cecil;

namespace AssemblyMethodsVirtualizer.MarkingStrategies
{
  /// <summary>
  /// A marking strategy has to offer the functionality of adding a custom attribute to the given method.
  /// All strategies have to implement this interface.
  /// </summary>
  public interface IMarkingAttributeStrategy
  {
    void AddCustomAttribute (MethodDefinition methodDefinition, AssemblyDefinition assemblyOfMethod);
  }
}