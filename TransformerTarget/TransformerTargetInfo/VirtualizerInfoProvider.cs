// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Reflection;

namespace TransformerTargetInfo
{
  public static class VirtualizerInfoProvider
  {
    public static bool ShouldVirtualizeType (Type t)
    {
      return true;
    }

    public static ConstructorInfo GetVirtualizedAttribute ()
    {
      return typeof (ReferalAttribute).GetConstructors()[0];
    }

    public static string GetVirtualizedName (MethodInfo m)
    {
      return "<>blah_" + m.Name;
    }
  }
}