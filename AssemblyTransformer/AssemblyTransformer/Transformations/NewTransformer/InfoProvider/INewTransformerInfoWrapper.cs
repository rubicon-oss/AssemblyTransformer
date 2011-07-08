// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;

namespace NewTransformer.InfoProvider
{
  public interface INewTransformerInfoWrapper
  {
    MethodDefinition GetFactoryMethod (MethodReference ctor, AssemblyDefinition assembly, IAssemblyTracker tracker);
    string GetWrapperMethodName (MethodReference ctor);
  }
}