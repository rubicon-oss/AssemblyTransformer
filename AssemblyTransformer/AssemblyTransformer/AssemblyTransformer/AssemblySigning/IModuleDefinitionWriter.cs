// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using Mono.Cecil;

namespace AssemblyTransformer.AssemblySigning
{
  public interface IModuleDefinitionWriter
  {
    void WriteModule (ModuleDefinition moduleDefinition, WriterParameters parameters);
  }
}