// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq;
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;

namespace ConstructorGenerator.ReferenceGenerator
{
  public class MethodReferenceGeneratorWithImport : IReferenceGenerator
  {
    private AssemblyNameReference _remotionInterfacesReference;
    private string _objectFactoryName;
    private string _objectFactoryNamespace;
    private string _paramListName;
    private string _paramListNamespace;
    private string _workingDir;

    public MethodReferenceGeneratorWithImport (string workingDir, string remotionInterfacesAssemblyName, string objectFactoryFullName, string paramListFullName)
    {
      _remotionInterfacesReference = AssemblyNameReference.Parse (remotionInterfacesAssemblyName);
      // ugly workaround for Cecils (wrong) behaviour concerning Culture
      if (_remotionInterfacesReference.Culture == "neutral")
        _remotionInterfacesReference.Culture = null;

      _objectFactoryName = objectFactoryFullName.Substring (
        objectFactoryFullName.LastIndexOf ('.') + 1, objectFactoryFullName.Length - objectFactoryFullName.LastIndexOf ('.') - 1);

      _objectFactoryNamespace = objectFactoryFullName.Substring (0, objectFactoryFullName.LastIndexOf ('.'));

      _paramListName = paramListFullName.Substring (
        paramListFullName.LastIndexOf ('.') + 1, paramListFullName.Length - paramListFullName.LastIndexOf ('.') - 1);

      _paramListNamespace = paramListFullName.Substring (0, paramListFullName.LastIndexOf ('.'));

      _workingDir = workingDir;
    }

    public MethodReference GetCallableObjectFactoryCreateMethod (AssemblyDefinition assemblyDef, ModuleDefinition moduleDefinition, TypeReference instantiatedType, IAssemblyTracker tracker)
    {
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

    private AssemblyNameReference GetOrCreateRemotionInterfacesReference (AssemblyDefinition assemblyDef, ModuleDefinition moduleDefinition, IAssemblyTracker tracker)
    {
      var tempRef = moduleDefinition.AssemblyReferences.FirstOrDefault (r => r.FullName == _remotionInterfacesReference.FullName);
      if (tempRef == null)
      {
        var assm = AssemblyDefinition.ReadAssembly (_workingDir + _remotionInterfacesReference.Name + ".dll");
        moduleDefinition.AssemblyReferences.Add (_remotionInterfacesReference);
        tracker.TrackNewReference (assemblyDef, _remotionInterfacesReference);
        return _remotionInterfacesReference;
      }
      return tempRef;
    }

    private MethodReference GetOrCreateParamList (int numOfParams, ModuleDefinition module, AssemblyNameReference assmRef)
    {
      if (numOfParams > 19)
        throw new NotSupportedException ("ParamList with more than 19 arguments does not exist.");

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
  }
}