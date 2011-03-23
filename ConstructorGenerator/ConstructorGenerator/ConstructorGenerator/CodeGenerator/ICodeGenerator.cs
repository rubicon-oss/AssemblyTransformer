// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;

namespace ConstructorGenerator.CodeGenerator
{
  public interface ICodeGenerator
  {
    bool ReplaceNewStatements (AssemblyDefinition containingAssembly, TypeDefinition containingType, MethodDefinition targetMethod, IAssemblyTracker tracker);
    void CreateNewObjectMethod (AssemblyDefinition containingAssembly, MethodDefinition templateMethod, IAssemblyTracker tracker);
  }
}