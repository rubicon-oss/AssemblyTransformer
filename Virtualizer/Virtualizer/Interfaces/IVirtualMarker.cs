// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MarkVirtual
{
  public interface IVirtualMarker
  {
    void OverrideMethods (ref ICollection<AssemblyManager> targets, Regex methodTargets, bool setVirtual);
    void Serialize (ICollection<AssemblyManager> targets);
  }
}