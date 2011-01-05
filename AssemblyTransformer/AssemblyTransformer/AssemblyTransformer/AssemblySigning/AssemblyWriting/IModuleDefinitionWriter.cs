// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using Mono.Cecil;

namespace AssemblyTransformer.AssemblySigning.AssemblyWriting
{
  /// <summary>
  /// A module definition writer, only has to offer the writing functionality.
  /// </summary>
  public interface IModuleDefinitionWriter
  {
    void WriteModule (ModuleDefinition moduleDefinition);
  }
}