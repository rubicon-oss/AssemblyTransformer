// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;

namespace TransformerTargetInfo
{
  public class ReferalAttribute : Attribute
  {
    public string VirtualMethodName;
    public ReferalAttribute (string virtualMethodName)
    {
      VirtualMethodName = virtualMethodName;
    }
  }
}