// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Remotion.Mixins;
using Remotion.Mixins.BridgeImplementations;
using Remotion.Reflection;

namespace TransformerTargetInfo
{
  public static class NewInfoProvider
  {
    private static List<string> _targetNamespaces = null;
    private static string _namespaceFilename = "potentiallyMixedNamespaces.txt";

    public static MethodBase GetFactoryMethod (ConstructorInfo ctor)
    {
      if (IsMixedType (ctor.DeclaringType))
        return typeof (NewInfoProvider).GetMethod ("Create", new []{typeof(ParamList)});
      else
        return null;
    }

    public static T Create<T> (ParamList p)
    {
      // Force Remotion.dll to be referenced. This is needed to include Remotion.dll with the TransformerTarget binaries.
      // Otherwise, TransformerTarget will not run.
      Type dummy = typeof (ObjectFactoryImplementation);
      
      return ObjectFactory.Create<T> (p, new object[0]);
    }

    public static string GetWrapperName (ConstructorInfo ctor)
    {
      return "WrapperName_" + ctor.Name;
    }

    private static bool IsMixedType (Type t)
    {
      if (_targetNamespaces == null)
      {
        _targetNamespaces = new List<string>(File.ReadLines (
            Path.Combine (Path.GetDirectoryName (Assembly.GetExecutingAssembly().Location), 
                          _namespaceFilename)));
      }
      Console.WriteLine (Path.Combine (Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location),
                          _namespaceFilename));
      return _targetNamespaces.Contains (t.Namespace);
      // mixinconfiguration: (requires that all projects are loaded)
      //return MixinConfiguration.ActiveConfiguration.GetContext (t) != null;
    }
  }
}