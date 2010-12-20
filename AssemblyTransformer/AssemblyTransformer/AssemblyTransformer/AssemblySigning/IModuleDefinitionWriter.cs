// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblySigning
{
  public interface IModuleDefinitionWriter
  {
    void WriteModule (ModuleDefinition moduleDefinition);
    void WriteModule (ModuleDefinition mainModule, ModuleDefinition moduleDefinition, List<StrongNameKeyPair> keys, StrongNameKeyPair defaultKey);
  }
}