// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;

namespace MethodVirtualizer.InfoProvider
{
  public interface IMethodVirtualizerInfoWrapper
  {
    bool ShouldVirtualizeType (TypeReference targetType);
    bool ShouldVirtualizeMethod (MethodDefinition targetMethod);
    MethodDefinition GetVirtualizedAttribute (AssemblyDefinition assembly, IAssemblyTracker tracker);
    string GetUnspeakableMethodName (MethodDefinition targetMethod);
  }
}