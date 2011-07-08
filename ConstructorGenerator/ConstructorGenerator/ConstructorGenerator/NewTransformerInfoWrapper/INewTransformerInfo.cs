// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Reflection;
using Mono.Cecil;

namespace ConstructorGenerator.NewTransformerInfoWrapper
{
  public interface INewTransformerInfo
  {
    MethodInfo RetrieveGetFactoryMethodFunc (string methodName);
    MethodInfo RetrieveGetWrapperNameFunc (string methodName);
  }
}