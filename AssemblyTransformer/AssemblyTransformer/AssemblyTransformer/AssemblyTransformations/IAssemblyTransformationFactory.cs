// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer.AppDomainBroker;

namespace AssemblyTransformer.AssemblyTransformations
{
  /// <summary>
  /// Every transformation has to be instantiated by a corresponding factory. This factory has to implement this interface.
  /// The factory has to add its required parameters to the optionset before the CreateTransformation is called, otherwise the
  /// parameters will be ommited.
  /// </summary>
  public interface IAssemblyTransformationFactory
  {
    void AddOptions (OptionSet options);
    IAssemblyTransformation CreateTransformation (IAppDomainInfoBroker broker);
  }
}