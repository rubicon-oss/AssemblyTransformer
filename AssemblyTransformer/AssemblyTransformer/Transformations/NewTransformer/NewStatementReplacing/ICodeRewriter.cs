// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;
using NewTransformer.InfoProvider;

namespace NewTransformer.NewStatementReplacing
{
  public interface ICodeRewriter
  {
    bool ReplaceNewStatements (
        AssemblyDefinition containingAssembly,
        TypeDefinition containingType,
        MethodDefinition targetMethod,
        IAssemblyTracker tracker,
        INewTransformerInfoWrapper infoWrapper);


    void CreateNewObjectMethod (
        AssemblyDefinition assembly, 
        MethodDefinition method, 
        IAssemblyTracker tracker, 
        INewTransformerInfoWrapper infoWrapper);
  }
}