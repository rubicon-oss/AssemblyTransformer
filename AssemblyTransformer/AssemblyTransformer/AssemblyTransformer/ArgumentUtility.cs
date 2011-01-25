// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;

namespace AssemblyTransformer
{
  public static class ArgumentUtility
  {
    public static void CheckNotNull<T> (string parameterName, T parameterValue)
    {
      if (null == parameterValue)
        throw new ArgumentNullException (parameterName);
    }
  }

  public class ProgramArgumentException : Exception
  {
    public ProgramArgumentException (string message, Exception innerException)
      : base ("AssemblyTransformer program argument error: " + message, 
      innerException) {}

    public ProgramArgumentException (string message)
      : base ("AssemblyTransformer program argument error: " + message) {}
  }

}