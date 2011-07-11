// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using ExampleLib;
using Remotion.Mixins;

namespace MyMixin
{

  [Extends(typeof(MixinTargetClass))]
  public class MyMixinClass : IMyMixin
  {
    public double CalculateAnother ()
    {
      return 11.1;
    }
  }

  public interface IMyMixin
  {
    double CalculateAnother ();
  }

}