using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace ConstructorGenerator.ReferenceGenerator
{
  public interface IReferenceGenerator
  {
    MethodReference GetCallableObjectFactoryCreateMethod (AssemblyDefinition assemblyDef, ModuleDefinition moduleDefinition, TypeReference instantiatedType, IAssemblyTracker tracker);
    MethodReference GetCallableParamListCreateMethod (AssemblyDefinition assemblyDef, MethodReference ctor, IAssemblyTracker tracker);
  }
}