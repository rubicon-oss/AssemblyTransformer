// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using Mono.Cecil;

namespace AssemblyTransformer.AssemblySigning.AssemblyWriting
{
  public interface IModuleDefinitionWriter
  {
    // TODO Review FS: I'd reverse the parameters: First what should be written (moduleDefinition), then the associated main module
    void WriteModule (ModuleDefinition mainModule, ModuleDefinition moduleDefinition);
  }
}