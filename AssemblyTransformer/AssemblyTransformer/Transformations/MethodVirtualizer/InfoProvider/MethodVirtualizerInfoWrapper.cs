// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using AssemblyTransformer.AppDomainBroker;
using AssemblyTransformer.AssemblyTracking;
using Mono.Cecil;

namespace MethodVirtualizer.InfoProvider
{
  public class MethodVirtualizerInfoWrapper : IMethodVirtualizerInfoWrapper
  {
    private readonly MethodVirtualizerInfoProvider _infoProvider;
    private bool _useAttribute = true;
    private MethodDefinition _attributeCtor = null;
    private AssemblyNameReference _customAttributeAssembly = null;

    public MethodVirtualizerInfoWrapper (MethodVirtualizerInfoProvider provider)
    {
      _infoProvider = provider;
    }

    public bool ShouldVirtualizeType (TypeReference targetType)
    {
      return _infoProvider.ShouldVirtualizeType (CecilResolver.CreateMemberID (targetType));
    }

    public bool ShouldVirtualizeMethod (MethodDefinition targetMethod)
    {
      return _infoProvider.ShouldVirtualizeMethod (CecilResolver.CreateMemberID (targetMethod));
    }

    public MethodDefinition GetVirtualizedAttribute (AssemblyDefinition assembly, IAssemblyTracker tracker)
    {
      if (!_useAttribute)
        return null;
      if (_attributeCtor == null)
      {
        var attributeCtorID = _infoProvider.GetVirtualizedAttribute ();
        _attributeCtor = CecilResolver.ResolveToMethodDefinition (attributeCtorID, tracker, out _customAttributeAssembly);
        if (_attributeCtor == null)
        {
          _useAttribute = false;
          return null;
        }
        if (_attributeCtor.Parameters.Count != 1)
          throw new ArgumentException("MethodVirtualizer: The given custom attribute does not have one (string) ctor parameter.");
      }
      
      if (_customAttributeAssembly != null)
        tracker.TrackNewReference (assembly, _customAttributeAssembly);
      return _attributeCtor;
    }

    public string GetUnspeakableMethodName (MethodDefinition targetMethod)
    {
      return _infoProvider.GetUnspeakableMethodName (CecilResolver.CreateMemberID (targetMethod), targetMethod.Name);
    }
  }
}