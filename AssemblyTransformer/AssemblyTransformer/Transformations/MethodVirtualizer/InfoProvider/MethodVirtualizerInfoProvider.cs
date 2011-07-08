// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq;
using System.Reflection;
using AssemblyTransformer.AppDomainBroker;

namespace MethodVirtualizer.InfoProvider
{
  public class MethodVirtualizerInfoProvider : MarshalByRefObject
  {
    private readonly Type _infoProvider;
    private readonly MethodInfo _virtualizeTypeMethod;
    private readonly MethodInfo _virtualizeMethodMethod;
    private readonly MethodInfo _virtualizedAttributeMethod;
    private readonly MethodInfo _virtualizedNameMethod;

    public MethodVirtualizerInfoProvider (string infoProviderName, string memberIDAssemblyCodeBase)
    {
      try
      {
        _infoProvider = Type.GetType (infoProviderName, true);
        _virtualizeTypeMethod = _infoProvider.GetMethod ("ShouldVirtualizeType", new[] { typeof (Type) });
      }
      catch (Exception e)
      {
        Console.WriteLine ("NewTransformer encountered a problem loading the type " + infoProviderName);
        throw;
      }
      try
      {
        _virtualizeMethodMethod = _infoProvider.GetMethod ("ShouldVirtualizeMethod", new[] { typeof (MethodInfo) });
      }
      catch (Exception) { _virtualizeMethodMethod = null; }

      try
      {
        _virtualizedAttributeMethod = _infoProvider.GetMethod ("GetVirtualizedAttribute", Type.EmptyTypes);
      }
      catch (Exception) { _virtualizedAttributeMethod = null; }

      try
      {
        _virtualizedNameMethod = _infoProvider.GetMethod ("GetVirtualizedName", new[] { typeof (MethodInfo) });
      }
      catch (Exception) { _virtualizedNameMethod = null; }
    }

    public bool ShouldVirtualizeType (MemberID targetType)
    {
      var type = Type.GetType (targetType.AssemblyQualifiedTypeName);
      return (bool) _virtualizeTypeMethod.Invoke (null, new[] { type });
    }

    public bool ShouldVirtualizeMethod (MemberID targetMethod)
    {
      if (_virtualizeMethodMethod == null)
        return true;

      var type = Type.GetType (targetMethod.AssemblyQualifiedTypeName);
      var methodInfo = type.GetMethods ().FirstOrDefault (c => c.MetadataToken == targetMethod.Token);
      return (bool) _virtualizeMethodMethod.Invoke (null, new[] { methodInfo });
    }

    public MemberID GetVirtualizedAttribute ()
    {
      if (_virtualizedAttributeMethod == null)
        return null;

      var result = (ConstructorInfo) _virtualizedAttributeMethod.Invoke (null, null);
      return new MemberID (result.DeclaringType.AssemblyQualifiedName, result.Module.FullyQualifiedName, result.MetadataToken);
    }

    public string GetUnspeakableMethodName (MemberID targetMethod, string name)
    {
      if (_virtualizedNameMethod == null)
        return "<>virtualized_" + name;

      var type = Type.GetType (targetMethod.AssemblyQualifiedTypeName);
      var methodInfo = type.GetMethods ().FirstOrDefault (c => c.MetadataToken == targetMethod.Token);
      return (string) _virtualizedNameMethod.Invoke (null, new[] { methodInfo });
    }
  }
}