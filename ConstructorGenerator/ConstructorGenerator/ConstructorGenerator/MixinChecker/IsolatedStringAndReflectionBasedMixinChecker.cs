// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Diagnostics;
using System.Reflection;
using Remotion.Implementation;

namespace ConstructorGenerator.MixinChecker
{
  public class IsolatedStringAndReflectionBasedMixinChecker : MarshalByRefObject
  {
    private readonly object _mixinConfiguration;
    private readonly MethodInfo _getContextMethod;

    public static IsolatedStringAndReflectionBasedMixinChecker Create (string workingDirectory, string remotionInterfacesFullName)
    {
      var setupInfo = AppDomain.CurrentDomain.SetupInformation;
      setupInfo.ApplicationBase = workingDirectory;
      AppDomain newDomain = AppDomain.CreateDomain ("MixinConfiguration checker", null, setupInfo);
      var checker = (IsolatedStringAndReflectionBasedMixinChecker) newDomain.CreateInstanceFromAndUnwrap (
                          typeof (IsolatedStringAndReflectionBasedMixinChecker).Assembly.CodeBase,
                          typeof (IsolatedStringAndReflectionBasedMixinChecker).FullName,
                          false,
                          BindingFlags.Public | BindingFlags.Instance,
                          null,
                          new[] { remotionInterfacesFullName },
                          null,
                          null);
      return checker;
    }

    public IsolatedStringAndReflectionBasedMixinChecker (string remotionInterfacesFullName)
    {
      var remotionInterfacesAssembly = Assembly.Load (remotionInterfacesFullName);
      FrameworkVersion.Value = remotionInterfacesAssembly.GetName().Version;
      var mixinConfigurationType = Remotion.Implementation.TypeNameTemplateResolver.ResolveToType (
              "Remotion.Mixins.MixinConfiguration, Remotion, Version=<version>, Culture=neutral, PublicKeyToken=<publicKeyToken>");
      _mixinConfiguration = mixinConfigurationType.InvokeMember (
              "ActiveConfiguration", BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty, null, null, null);
      _getContextMethod = mixinConfigurationType.GetMethod ("GetContext", new[] { typeof (Type) });
    }

    public bool CanBeMixed (string fullName)
    {
      Type type;
      try
      {
        type = Type.GetType (fullName, true);
      }
      catch (Exception e)
      {
        Console.WriteLine ("failed Type.GetType() lookup :\n" + e);
        throw;
      }
      return _getContextMethod.Invoke (_mixinConfiguration, new[] { type }) != null;
    }
  }
}