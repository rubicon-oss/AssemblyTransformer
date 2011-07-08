// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//

namespace AssemblyTransformer.AppDomainBroker
{
  public interface IAppDomainInfoBroker
  {
    T CreateInfoProviderWrapper<T> (params object[] args);
    void Unload ();
  }
}