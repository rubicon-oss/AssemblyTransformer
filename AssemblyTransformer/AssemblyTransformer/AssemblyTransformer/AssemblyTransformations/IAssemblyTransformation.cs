// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Text.RegularExpressions;

namespace AssemblyTransformer.AssemblyTransformations
{
  public interface IAssemblyTransformation
  {
    void Transform (IAssemblyTracker tracker);
  }
}