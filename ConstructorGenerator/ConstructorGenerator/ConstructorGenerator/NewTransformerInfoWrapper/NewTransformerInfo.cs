// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Reflection;
using Mono.Cecil;


namespace ConstructorGenerator.NewTransformerInfoWrapper
{
  public class NewTransformerInfo : MarshalByRefObject
  {
    private readonly Type _infoProvider;

    public NewTransformerInfo (string infoProviderName)
    {
      try
      {
        _infoProvider = Type.GetType (infoProviderName, true);
      }
      catch (Exception e)
      {
        Console.WriteLine ("NewTransformer encountered a problem loading the type " + infoProviderName);
        throw;
      }
    }

    public MethodInfo RetrieveGetFactoryMethodFunc (string methodName)
    {
      var method = _infoProvider.GetMethod ("GetFactoryMethod", new[] { typeof (ConstructorInfo) });
      return method;
    }

    public MethodInfo RetrieveGetWrapperNameFunc (string methodName)
    {
      var method = _infoProvider.GetMethod ("GetWrapperName", new[] { typeof (ConstructorInfo) });
      return method;
    }
  }
}