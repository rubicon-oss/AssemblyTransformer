// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer.AssemblyTracking;

namespace AssemblyTransformer.AssemblyTransformations
{
  /// <summary>
  /// This is the interface which has to be implemented by all the transformations that should be executed
  /// on the given assemblytracker's assemblies.
  /// The Transform method will be executed only once per transformation and the execution of all transformations
  /// is carried out serial.
  /// </summary>
  public interface IAssemblyTransformation
  {
    void Transform (IAssemblyTracker tracker);
  }
}