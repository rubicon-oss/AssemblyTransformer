// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyTransformer;
using Mono.Cecil;

namespace AssemblyMethodsVirtualizer.MarkingStrategies
{
  /// <summary>
  /// The abstract baseclass of all marking strategies provides nontrivial core functionality and the action flow of the necessary
  /// steps to add references/types and the corresponding attributes to the method/module.
  /// The concrete strategies have to implement the CreateCustomAttributeType method which generates/adds the type to the given module.
  /// </summary>
  public abstract class MarkingAttributeStrategy : IMarkingAttributeStrategy
  {
    protected readonly string _attributeNamespace;
    protected readonly string _attributeName;

    public string AttributeNamespace
    {
      get { return _attributeNamespace; }
    }

    public string AttributeName
    {
      get { return _attributeName; }
    }

    protected MarkingAttributeStrategy (string attributeNamespace, string attributeName)
    {
      ArgumentUtility.CheckNotNull ("attributeNamespace", attributeNamespace);
      ArgumentUtility.CheckNotNull ("attributeName", attributeName);

      _attributeNamespace = attributeNamespace;
      _attributeName = attributeName;
    }

    protected abstract TypeDefinition CreateCustomAttributeType (ModuleDefinition targetModule);

    public abstract void AddCustomAttribute (MethodDefinition methodDefinition, AssemblyDefinition assemblyOfMethod);

    protected void AddCustomAttribute (MethodDefinition methodDefinition, ModuleDefinition moduleWithAttributeType)
    {
      ArgumentUtility.CheckNotNull ("methodDefinition", methodDefinition);
      ArgumentUtility.CheckNotNull ("moduleWithAttributeType", moduleWithAttributeType);

      var attributeCtor = MakeCtorAndReference (methodDefinition.Module, moduleWithAttributeType);
      var customAttribute = new CustomAttribute (attributeCtor);

      if (!methodDefinition.CustomAttributes.Any (att => att.Constructor.FullName == attributeCtor.FullName))
        methodDefinition.CustomAttributes.Add (customAttribute);
    }

    protected void AddCustomAttribute (MethodDefinition methodDefinition, ModuleDefinition moduleWithAttributeType, 
      CustomAttributeArgument ctorArgument)
    {
      ArgumentUtility.CheckNotNull ("methodDefinition", methodDefinition);
      ArgumentUtility.CheckNotNull ("moduleWithAttributeType", moduleWithAttributeType);

      var attributeCtor = MakeCtorAndReference ( methodDefinition.DeclaringType.Module, moduleWithAttributeType);
      if (attributeCtor.Parameters.Count != 1)
        throw new InvalidOperationException ("There is no custom attribute ctor available that takes one parameter!");
      
      var customAttribute = new CustomAttribute (attributeCtor);
      customAttribute.ConstructorArguments.Add (ctorArgument);

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

      var ctorReference = targetModule.Import (attributeCtorDefinition);

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