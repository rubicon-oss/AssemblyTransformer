// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
namespace AssemblyTransformer
{
  public interface IAssemblySigner
  {
    void SignAndSave (IAssemblyTracker tracker);
  }
}