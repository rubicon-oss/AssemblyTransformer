// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mono.Cecil;

namespace ConstructorGenerator.MixinChecker
{
  /// <summary>
  /// in this case a wrapper for the isolated mixin checker, which is needed because of AppDomain etc.
  /// </summary>
  public class StringAndReflectionBasedMixinChecker : IMixinChecker
  {
    private readonly IsolatedStringAndReflectionBasedMixinChecker _isolatedMixinChecker;
    private readonly Dictionary<string, bool> _mixableTypesCache;

    public StringAndReflectionBasedMixinChecker (string workingDir, string remotionInterfacesFullName)
    {
      _isolatedMixinChecker = IsolatedStringAndReflectionBasedMixinChecker.Create (workingDir, remotionInterfacesFullName);
      _mixableTypesCache = new Dictionary<string, bool>();
    }

    public bool CanBeMixed (string assemblyQualifiedTypeName)
    {
      if (assemblyQualifiedTypeName == null || assemblyQualifiedTypeName.StartsWith ("<Module>"))
        return false;
      if (!IsCached (assemblyQualifiedTypeName))
        _mixableTypesCache[assemblyQualifiedTypeName] = _isolatedMixinChecker.CanBeMixed (assemblyQualifiedTypeName);
      return _mixableTypesCache[assemblyQualifiedTypeName];
    }

    public bool IsCached (string assemblyQualifiedTypeName)
    {
      return _mixableTypesCache.ContainsKey (assemblyQualifiedTypeName);
    }
  }
}