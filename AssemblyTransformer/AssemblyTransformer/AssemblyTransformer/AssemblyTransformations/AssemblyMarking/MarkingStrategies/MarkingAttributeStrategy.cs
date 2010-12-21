// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Linq;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblyTransformations.AssemblyMarking.MarkingStrategies
{
  public abstract class MarkingAttributeStrategy : IMarkingAttributeStrategy
  {
    protected readonly string _attributeNamespace;
    protected readonly string _attributeName;

    protected MarkingAttributeStrategy (string attributeNamespace, string attributeName)
    {
      _attributeNamespace = attributeNamespace;
      _attributeName = attributeName;
    }

    protected abstract TypeDefinition CreateCustomAttributeType (ModuleDefinition targetModule);

    public abstract void AddCustomAttribute (MethodDefinition methodDefinition, AssemblyDefinition assemblyOfMethod);

    protected void AddCustomAttribute (MethodDefinition methodDefinition, ModuleDefinition moduleWithAttributeType)
    {
      var attributeCtor = MakeCtorAndReference ( methodDefinition.Module, moduleWithAttributeType);
      var customAttribute = new CustomAttribute (attributeCtor);
      if (!methodDefinition.CustomAttributes.Any (att => att.Constructor.FullName == attributeCtor.FullName))
        methodDefinition.CustomAttributes.Add (customAttribute);
    }

    private MethodReference MakeCtorAndReference (ModuleDefinition targetModule, ModuleDefinition moduleWithAttributeType)
    {
      // optimization - trygettypereference does not work in case of in memory reference...
      //TypeReference existingAttributeReference;
      //if (targetModule.TryGetTypeReference (_attributeNamespace + "." + _attributeName, out existingAttributeReference))
      //  return (MethodReference) targetModule.GetMemberReferences ().Single (mr => mr.DeclaringType == existingAttributeReference);

      var attributeCtorDefinition = GetCustomAttributeCtor (moduleWithAttributeType);
      if (targetModule == moduleWithAttributeType)
        return attributeCtorDefinition;

      var moduleReference = new ModuleReference (moduleWithAttributeType.Name);

      if (!targetModule.ModuleReferences.Contains (moduleReference))
        targetModule.ModuleReferences.Add (moduleReference);

      var typeReference = new TypeReference (
          attributeCtorDefinition.DeclaringType.Namespace,
          attributeCtorDefinition.DeclaringType.Name,
          moduleReference);
      var ctorReference = new MethodReference (
          attributeCtorDefinition.Name,
          targetModule.Import (attributeCtorDefinition.ReturnType),
          typeReference);
      ctorReference.HasThis = true;

      return ctorReference;
    }

    private MethodDefinition GetCustomAttributeCtor (ModuleDefinition moduleWithAttributeType)
    {
      var existingAttributeType = moduleWithAttributeType.Types.FirstOrDefault (td => td.FullName == _attributeNamespace + "." + _attributeName);
      if (existingAttributeType == null)
      {
        existingAttributeType = CreateCustomAttributeType (moduleWithAttributeType);
        moduleWithAttributeType.Types.Add (existingAttributeType);
      }
      return existingAttributeType.Methods.Single ();
    }
  }
}