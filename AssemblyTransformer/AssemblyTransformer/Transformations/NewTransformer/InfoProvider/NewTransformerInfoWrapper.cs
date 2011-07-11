// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using AssemblyTransformer.AppDomainBroker;
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;

namespace NewTransformer.InfoProvider
{
  public class NewTransformerInfoWrapper : INewTransformerInfoWrapper
  {
    private readonly NewTransformerInfoProvider _infoProvider;
    private readonly Dictionary<MethodReference, MethodDefinition> _factoryMethods;

    public NewTransformerInfoWrapper (NewTransformerInfoProvider provider)
    {
      _infoProvider = provider;
      _factoryMethods = new Dictionary<MethodReference, MethodDefinition> ();
    }

    public MethodDefinition GetFactoryMethod (MethodReference ctor, AssemblyDefinition assembly, IAssemblyTracker tracker)
    {
      if (!_factoryMethods.ContainsKey (ctor))
      {
        var memberId = CecilResolver.CreateMemberID (ctor.Resolve());
        AssemblyNameReference containingTrackedAssembly;
        _factoryMethods[ctor] = CecilResolver.ResolveToMethodDefinition (_infoProvider.GetFactoryMethodFunc (memberId), tracker, out containingTrackedAssembly);
        if (containingTrackedAssembly != null)
          tracker.TrackNewReference (assembly, containingTrackedAssembly);
      }
      return _factoryMethods[ctor];
    }

    public string GetWrapperMethodName (MethodReference ctor)
    {
      var memberId = CecilResolver.CreateMemberID (ctor);
      return _infoProvider.GetWrapperName (memberId);
    }
  }
}