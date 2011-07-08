// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using AssemblyTransformer.AppDomainBroker;

namespace NewTransformer.InfoProvider
{
  public class NewTransformerInfoProvider : MarshalByRefObject
  {
    private readonly Type _infoProvider;
    private readonly MethodInfo _getFactoryMethod;
    private readonly MethodInfo _getWrapperMethod;

    public NewTransformerInfoProvider (string infoProviderName, string memberIDAssemblyCodeBase)
    {
      try
      {
        _infoProvider = Type.GetType (infoProviderName, true);
        _getWrapperMethod = _infoProvider.GetMethod ("GetWrapperName", new[] { typeof (ConstructorInfo) });
        _getFactoryMethod = _infoProvider.GetMethod ("GetFactoryMethod", new[] { typeof (ConstructorInfo) });
      }
      catch (Exception e)
      {
        Console.WriteLine ("NewTransformer encountered a problem loading the type " + infoProviderName);
        throw;
      }
    }

    public MemberID GetFactoryMethodFunc (MemberID ctorID)
    {
      var type = Type.GetType (ctorID.AssemblyQualifiedTypeName);
      var ctorInfo = type.GetConstructors().FirstOrDefault (c => c.MetadataToken == ctorID.Token);
      var result = (MethodBase) _getFactoryMethod.Invoke (null, new[] { ctorInfo });
      if (result == null)
        return null;
      return new MemberID (result.DeclaringType.AssemblyQualifiedName, result.Module.FullyQualifiedName, result.MetadataToken);
    }

    public string GetWrapperName (MemberID ctorID)
    {
      var type = Type.GetType (ctorID.AssemblyQualifiedTypeName);
      var ctorInfo = type.GetConstructors().FirstOrDefault(c => c.MetadataToken == ctorID.Token);
      return (string)_getWrapperMethod.Invoke (null, new[] { ctorInfo });
    }
    
  }
}