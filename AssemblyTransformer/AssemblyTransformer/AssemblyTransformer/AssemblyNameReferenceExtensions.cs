// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using Mono.Cecil;
using System.Linq;

namespace AssemblyTransformer
{
  public static class AssemblyNameReferenceExtensions
  {
    public static bool MatchesDefinition (this AssemblyNameReference reference, AssemblyNameReference definition)
    {
      return  reference.Name == definition.Name 
          && (reference.Culture == null || reference.Culture == definition.Culture) 
          && (reference.Version == null || reference.Version == definition.Version)
          && (reference.PublicKeyToken == null || (definition.PublicKeyToken != null && reference.PublicKeyToken.SequenceEqual (definition.PublicKeyToken)));
    }
  }
}