// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;

namespace AssemblyTransformer
{
  static class ArgumentUtility
  {
    public static void CheckNotNull<T> (string parameterName, T parameterValue)
    {
      if (null == parameterValue)
        throw new ArgumentNullException (parameterName);
    }
  }

}