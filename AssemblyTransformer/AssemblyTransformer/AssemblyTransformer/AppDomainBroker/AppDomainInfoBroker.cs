// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AssemblyTransformer.AppDomainBroker
{
  public class AppDomainInfoBroker : IAppDomainInfoBroker
  {
    private readonly AppDomain _appDomain;
    private readonly Dictionary<Type, object> _infoWrappers;
    private readonly AssemblyReferenceResolver _assemblyReferenceResolver;

    public AppDomainInfoBroker (string workingDir)
    {
      var setupInfo = AppDomain.CurrentDomain.SetupInformation;
      setupInfo.ApplicationBase = workingDir;
      _appDomain = AppDomain.CreateDomain ("AssemblyTransformer AppDomainInfoBroker", null, setupInfo);
      _assemblyReferenceResolver =
          (AssemblyReferenceResolver)
          _appDomain.CreateInstanceFromAndUnwrap (typeof (AssemblyReferenceResolver).Assembly.CodeBase, typeof (AssemblyReferenceResolver).FullName);

      _infoWrappers = new Dictionary<Type, object>();
      _assemblyReferenceResolver.Install (GetType().Assembly.CodeBase);
    }

    public T CreateInfoProviderWrapper<T> (params object[] args)
    {
      if (!_infoWrappers.ContainsKey (typeof (T)))
        _infoWrappers[typeof (T)] = (T) _appDomain.CreateInstanceFromAndUnwrap (
            typeof (T).Assembly.CodeBase,
            typeof (T).FullName,
            false,
            BindingFlags.Public | BindingFlags.Instance,
            null,
            args,
            null,
            null);
      return (T) _infoWrappers[typeof (T)];
    }

    public void Unload ()
    {
      AppDomain.Unload (_appDomain);
    }
  } 
}