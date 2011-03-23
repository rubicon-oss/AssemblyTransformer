// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using Mono.Cecil;

namespace ConstructorGenerator.MixinChecker
{
  public interface IMixinChecker
  {
    bool CanBeMixed (string assemblyQualifiedTypeName);
    bool IsCached (string assemblyQualifiedTypeName);
  }
}