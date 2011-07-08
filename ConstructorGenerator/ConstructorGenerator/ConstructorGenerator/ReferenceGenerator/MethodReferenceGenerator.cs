// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq;
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace ConstructorGenerator.ReferenceGenerator
{
  /// <summary>
  /// takes care of adding the needed assembly references and generates the needed methodreferences for the IL calls.
  /// </summary>
  public class MethodReferenceGenerator : IReferenceGenerator
  {
    private readonly string _remotionInterfacesAssemblyName;
    private readonly string _objectFactoryName;
    private readonly string _objectFactoryNamespace;
    private readonly string _paramListName;
    private readonly string _paramListNamespace;

    public string ObjectFactoryName
    {
      get { return _objectFactoryName; }
    }
    public string ObjectFactoryNamespace
    {
      get { return _objectFactoryNamespace; }
    }
    public string ParamListName
    {
      get { return _paramListName; }
    }
    public string ParamListNamespace
    {
      get { return _paramListNamespace; }
    }

    public MethodReferenceGenerator (string remotionInterfacesAssemblyName, string objectFactoryFullName, string paramListFullName)
    {
      _remotionInterfacesAssemblyName = remotionInterfacesAssemblyName;

      _objectFactoryName = objectFactoryFullName.Substring (
        objectFactoryFullName.LastIndexOf ('.') + 1, objectFactoryFullName.Length - objectFactoryFullName.LastIndexOf ('.') - 1);

      _objectFactoryNamespace = objectFactoryFullName.Substring (0, objectFactoryFullName.LastIndexOf ('.'));

      _paramListName = paramListFullName.Substring (
        paramListFullName.LastIndexOf ('.') + 1, paramListFullName.Length - paramListFullName.LastIndexOf ('.') - 1);

      _paramListNamespace = paramListFullName.Substring (0, paramListFullName.LastIndexOf ('.'));
    }

    public MethodReference GetCallableObjectFactoryCreateMethod (AssemblyDefinition assemblyDef, ModuleDefinition moduleDefinition, TypeReference instantiatedType, IAssemblyTracker tracker)
    {
      MethodReference reference;
      if (assemblyDef.FullName == _remotionInterfacesAssemblyName &&
          (reference = SearchObjectFactoryMethod (assemblyDef)) != null)
      {
        ((GenericInstanceMethod) reference).GenericArguments.Add (instantiatedType);
        return reference;
      }

      var tempRef = GetOrCreateRemotionInterfacesReference (assemblyDef, moduleDefinition, tracker);

      var objectFactoryReference = new TypeReference (_objectFactoryNamespace, _objectFactoryName, moduleDefinition, tempRef);
      var paramListReference = new TypeReference (_paramListNamespace, _paramListName, moduleDefinition, tempRef);

      var createReference = new MethodReference ("Create", moduleDefinition.TypeSystem.Void, objectFactoryReference);
      var createTypeParam = new GenericParameter ("T", createReference);

      createReference.GenericParameters.Add (createTypeParam);
      createReference.ReturnType = createTypeParam;
      createReference.Parameters.Add (new ParameterDefinition (moduleDefinition.TypeSystem.Boolean));
      createReference.Parameters.Add (new ParameterDefinition (paramListReference));
      createReference.Parameters.Add (new ParameterDefinition (new ArrayType (moduleDefinition.TypeSystem.Object)));

      var instanceMethod = new GenericInstanceMethod (createReference);
      instanceMethod.GenericArguments.Add (instantiatedType);

      return instanceMethod;
    }

    public MethodReference GetCallableParamListCreateMethod (AssemblyDefinition assemblyDef, MethodReference ctor, IAssemblyTracker tracker)
    {
      MethodReference reference;
      if (assemblyDef.FullName == _remotionInterfacesAssemblyName &&
          (reference = SearchParamListFactoryMethod (assemblyDef, ctor)) != null)
        return reference;

      var tempRef = GetOrCreateRemotionInterfacesReference (assemblyDef, ctor.DeclaringType.Module, tracker);
      var paramListCreateReference = GetOrCreateParamList (ctor.Parameters.Count, ctor.DeclaringType.Module, tempRef);

      if (ctor.Parameters.Count > 0)
      {
        paramListCreateReference = new GenericInstanceMethod (paramListCreateReference);

        foreach (var param in ctor.Parameters)
          ((GenericInstanceMethod) paramListCreateReference).GenericArguments.Add (param.ParameterType);
      }
      return paramListCreateReference;
    }

    public AssemblyNameReference GetOrCreateRemotionInterfacesReference (AssemblyDefinition assemblyDef, ModuleDefinition moduleDefinition, IAssemblyTracker tracker)
    {
      var tempRef = moduleDefinition.AssemblyReferences.FirstOrDefault (r => r.FullName == _remotionInterfacesAssemblyName);
      if (tempRef == null)
      {
        tempRef = AssemblyNameReference.Parse (_remotionInterfacesAssemblyName);
        // ugly workaround for Cecils (wrong) behaviour concerning Culture
        if (tempRef.Culture == "neutral")
          tempRef.Culture = null;

        moduleDefinition.AssemblyReferences.Add (tempRef);
        tracker.TrackNewReference (assemblyDef, tempRef);
      }
      return tempRef;
    }

    public MethodReference GetOrCreateParamList (int numOfParams, ModuleDefinition module, AssemblyNameReference assmRef)
    {
      if (numOfParams > 19)
        throw new NotSupportedException("ParamList with more than 19 arguments does not exist.");

      var paramListReference = new TypeReference (_paramListNamespace, _paramListName, module, assmRef);
      module.Import (paramListReference);
      var createReference = new MethodReference ("Create", paramListReference, paramListReference);

      for (int i = 0; i < numOfParams; ++i)
      {
        var createTypeParam = new GenericParameter (createReference);
        createReference.GenericParameters.Add (createTypeParam);
        createReference.Parameters.Add (new ParameterDefinition (createTypeParam));
      }
      return createReference;
    }

    private MethodReference SearchObjectFactoryMethod (AssemblyDefinition assemblyDef)
    {
      TypeDefinition t;
      foreach (var module in assemblyDef.Modules)
      {
        if ((t = module.GetType (_objectFactoryNamespace, _objectFactoryName)) != null)
        {
          var method =
            t.Methods.FirstOrDefault (m => m.Name == "Create" &&
                                          m.HasGenericParameters && m.Parameters.Count == 3 &&
                                          m.Parameters[0].ParameterType == assemblyDef.MainModule.TypeSystem.Boolean &&
                                          m.Parameters[1].ParameterType.Namespace == _paramListNamespace &&
                                          m.Parameters[1].ParameterType.Name == _paramListName &&
                                          m.Parameters[2].ParameterType.IsArray &&
                                          ((ArrayType) m.Parameters[2].ParameterType).ElementType == assemblyDef.MainModule.TypeSystem.Object);
          if (method != null)
            return new GenericInstanceMethod(method);
        }
      }
      return null;
    }

    private MethodReference SearchParamListFactoryMethod (AssemblyDefinition assemblyDef, MethodReference ctor)
    {
      TypeDefinition t;
      foreach (var module in assemblyDef.Modules)
      {
        if ((t = module.GetType (_paramListNamespace, _paramListName)) != null)
        {
          MethodReference method =
            t.Methods.FirstOrDefault (m => m.Name == "Create" &&
                                          m.Parameters.Count == ctor.Parameters.Count &&
                                          m.GenericParameters.Count == ctor.Parameters.Count);
          if (method != null)
          {
            if (method.HasGenericParameters)
            {
              method = new GenericInstanceMethod (method);
              foreach (var param in ctor.Parameters)
                ((GenericInstanceMethod)method).GenericArguments.Add (param.ParameterType);
            }
            return method;
          }
        }
      }
      return null;
    }
  }
}