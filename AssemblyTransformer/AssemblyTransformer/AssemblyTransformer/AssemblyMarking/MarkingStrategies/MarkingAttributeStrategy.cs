// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Linq;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblyMarking.MarkingStrategies
{
  public abstract class MarkingAttributeStrategy : IMarkingAttributeStrategy
  {
    protected readonly string _attributeNamespace;
    protected readonly string _attributeName;

    private MarkingAttributeStrategy () {}

    protected MarkingAttributeStrategy (string attributeNamespace, string attributeName)
    {
      _attributeNamespace = attributeNamespace;
      _attributeName = attributeName;
    }

    public void AddCustomAttribute (MethodDefinition methodDefinition, ModuleDefinition moduleWithAttributeType)
    {
      var attributeCtor = MakeCtorAndReference ( methodDefinition.Module, moduleWithAttributeType);
      var customAttribute = new CustomAttribute (attributeCtor);
      if (!methodDefinition.CustomAttributes.Contains (customAttribute))
        methodDefinition.CustomAttributes.Add (customAttribute);
    }

    private MethodReference GetCustomAttributeCtor (ModuleDefinition moduleWithAttributeType)
    {
        var existingAttributeType = moduleWithAttributeType.Types.FirstOrDefault (td => td.FullName == _attributeNamespace + "." + _attributeName);
        if (existingAttributeType == null)
        {
          existingAttributeType = CreateCustomAttributeType (moduleWithAttributeType);
          moduleWithAttributeType.Types.Add (existingAttributeType);
        }
        return existingAttributeType.Methods.Single ();
    }

    private MethodReference MakeCtorAndReference (ModuleDefinition targetModule, ModuleDefinition moduleWithAttributeType)
    {
      TypeReference existingAttributeReference;
      if (targetModule.TryGetTypeReference (_attributeNamespace + "." + _attributeName, out existingAttributeReference))
        return (MethodReference) targetModule.GetMemberReferences ().Single (mr => mr.DeclaringType == existingAttributeReference);

      var attributeCtorReference = GetCustomAttributeCtor (moduleWithAttributeType);
      var moduleReference = new ModuleReference (moduleWithAttributeType.Name);

      if (!targetModule.ModuleReferences.Contains (moduleReference))
        targetModule.ModuleReferences.Add (moduleReference);

      var typeReference = new TypeReference (
          attributeCtorReference.DeclaringType.Namespace,
          attributeCtorReference.DeclaringType.Name,
          moduleReference);
      var ctorReference = new MethodReference (
          attributeCtorReference.Name,
          targetModule.Import (attributeCtorReference.ReturnType),
          typeReference);
      ctorReference.HasThis = true;

      return ctorReference;
    }

    protected abstract TypeDefinition CreateCustomAttributeType (ModuleDefinition targetModule);

  }
}