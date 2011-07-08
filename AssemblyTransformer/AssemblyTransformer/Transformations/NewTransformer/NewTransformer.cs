// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Linq;
using AssemblyTransformer;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;
using AssemblyTransformer.Extensions;
using Mono.Cecil;
using NewTransformer.InfoProvider;
using NewTransformer.NewStatementReplacing;

namespace NewTransformer
{
  /// <summary>
  /// This transformation creates NewObject methods in all types that can be mixed.
  /// Then, all assemblies are scanned for new statements that isntantiate these potentially mixed types
  /// and replaces them with the generated NewObject methods.
  /// </summary>
  public class NewTransformer : IAssemblyTransformation
  {

    private IAssemblyTracker _tracker;
    private readonly INewTransformerInfoWrapper _infoWrapper;
    private readonly ICodeRewriter _replacer;
    private readonly bool _factoryMakeProtected;

    public NewTransformer (INewTransformerInfoWrapper infoWrapper, ICodeRewriter replacer, bool factoryMakeProtected)
    {
      ArgumentUtility.CheckNotNull ("infoWrapper", infoWrapper);
      ArgumentUtility.CheckNotNull ("replacer", replacer);
      _infoWrapper = infoWrapper;
      _replacer = replacer;
      _factoryMakeProtected = factoryMakeProtected;
    }

    public void Transform (IAssemblyTracker tracker)
    {
      ArgumentUtility.CheckNotNull ("tracker", tracker);
      _tracker = tracker;

      foreach (var assemblyDefinition in tracker.GetAssemblies ())
      {
        foreach (var typeDefinition in assemblyDefinition.LoadAllTypes ())
        {
          ReplaceNewStatement (assemblyDefinition, typeDefinition);
          if (_factoryMakeProtected)
            GenerateNewObjectMethods (assemblyDefinition, typeDefinition);
        }
      }
    }

    private void ReplaceNewStatement (AssemblyDefinition assembly, TypeDefinition analyzedType)
    {
      foreach (var targetMethod in analyzedType.Methods)
        if (_replacer.ReplaceNewStatements (assembly, analyzedType, targetMethod, _tracker, _infoWrapper))
          _tracker.MarkModified (assembly);
    }


    private void GenerateNewObjectMethods (AssemblyDefinition assembly, TypeDefinition targetType)
    {
      foreach (var method in targetType.Methods.ToList ())
        if (method.IsConstructor && !targetType.Methods.Any (m => (m.Name == "NewObject" && HaveSameParameters (method, m))))
        {
          // set ctor protected
          method.IsPublic = false;
          method.IsPrivate = false;
          method.IsFamily = true;
          _replacer.CreateNewObjectMethod (assembly, method, _tracker, _infoWrapper);
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
  }
}