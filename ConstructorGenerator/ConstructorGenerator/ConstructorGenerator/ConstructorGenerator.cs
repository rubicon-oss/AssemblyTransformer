// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Linq;
using AssemblyTransformer;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;
using AssemblyTransformer.Extensions;
using ConstructorGenerator.CodeGenerator;
using ConstructorGenerator.MixinChecker;
using Mono.Cecil;

namespace ConstructorGenerator
{
  /// <summary>
  /// This transformation creates NewObject methods in all types that can be mixed.
  /// Then, all assemblies are scanned for new statements that isntantiate these potentially mixed types
  /// and replaces them with the generated NewObject methods.
  /// </summary>
  public class ConstructorGenerator : IAssemblyTransformation
  {
    private readonly IMixinChecker _checker;
    private readonly ICodeGenerator _codeGenerator;

    private IAssemblyTracker _tracker;

    public IMixinChecker Checker
    {
      get { return _checker; }
    }

    public ICodeGenerator CodeGenerator
    {
      get { return _codeGenerator; }
    }

    public ConstructorGenerator (IMixinChecker checker, ICodeGenerator codeGenerator)
    {
      _checker = checker;
      _codeGenerator = codeGenerator;
    }

    public void Transform (IAssemblyTracker tracker)
    {
      ArgumentUtility.CheckNotNull ("tracker", tracker);
      _tracker = tracker;

      var mixinTargetTypes =  from assemblyDefinition in tracker.GetAssemblies ()
                              from typeDefinition in assemblyDefinition.LoadAllTypes()
                              where _checker.CanBeMixed (assemblyDefinition.Name.BuildReflectionAssemblyQualifiedName(typeDefinition))
                              select new { Assembly = assemblyDefinition, TypeDef = typeDefinition };

      foreach (var mixinTargetType in mixinTargetTypes.ToList ())
        GenerateNewObjectMethods (mixinTargetType.Assembly, mixinTargetType.TypeDef);

      foreach(var assembly in tracker.GetAssemblies())
        foreach (var typ in assembly.LoadAllTypes())
          ReplaceNewStatement (assembly, typ);
    }

    private void GenerateNewObjectMethods (AssemblyDefinition assembly, TypeDefinition targetType)
    {
      foreach (var method in targetType.Methods.ToList ())
        if (method.IsConstructor && !targetType.Methods.Any (m => (m.Name == "NewObject" && HaveSameParameters (method, m))))
        {
          _codeGenerator.CreateNewObjectMethod (assembly, method, _tracker);
          _tracker.MarkModified (assembly);
        }
    }

    private bool HaveSameParameters (MethodDefinition that, MethodDefinition other)
    {
      int count;
      if ((count = that.Parameters.Count) != other.Parameters.Count)
        return false;
      foreach (var param in that.Parameters.Where (param => other.Parameters.Any (p => p.ParameterType.FullName == param.ParameterType.FullName)))
        --count;
      return count == 0;
    }

    private void ReplaceNewStatement (AssemblyDefinition assembly, TypeDefinition analyzedType)
    {
      foreach (var targetMethod in analyzedType.Methods)
        if (_codeGenerator.ReplaceNewStatements (assembly, analyzedType, targetMethod, _tracker))
          _tracker.MarkModified (assembly);
    }
  } 
}