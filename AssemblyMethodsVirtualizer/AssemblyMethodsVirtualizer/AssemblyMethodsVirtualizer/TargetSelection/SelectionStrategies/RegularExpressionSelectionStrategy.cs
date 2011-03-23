// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Text.RegularExpressions;
using AssemblyTransformer;
using Mono.Cecil;

namespace AssemblyMethodsVirtualizer.TargetSelection.SelectionStrategies
{
  public class RegularExpressionSelectionStrategy : ITargetSelectionStrategy
  {
    private readonly Regex _regex;

    public RegularExpressionSelectionStrategy (string regex)
    {
      ArgumentUtility.CheckNotNull ("regex", regex);
      _regex = new Regex(regex);
    }

    public bool IsTarget (MethodDefinition method, AssemblyDefinition containingAssembly)
    {
      return _regex.IsMatch (method.FullName);
    }
  }
}