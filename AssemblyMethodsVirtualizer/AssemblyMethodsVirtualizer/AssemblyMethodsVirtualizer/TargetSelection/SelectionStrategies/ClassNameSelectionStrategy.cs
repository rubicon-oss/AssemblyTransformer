using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace AssemblyMethodsVirtualizer.TargetSelection.SelectionStrategies
{
  class ClassNameSelectionStrategy : IVisitorTargetSelectionStrategy
  {
    private readonly List<string> _classNames;

    public ClassNameSelectionStrategy (List<string> classNames)
    {
      _classNames = classNames;
    }

    public bool IsTarget (MethodDefinition method)
    {
      return false;
    }

    public bool AreAllMethodsOfTypeTarget (TypeDefinition typ)
    {
      return _classNames.Any (name => name == typ.FullName);
    }
  }
}
