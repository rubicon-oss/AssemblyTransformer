// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Collections.Generic;

namespace MarkVirtual
{
  public interface IModificationSerializer
  {
    void Serialize (ICollection<AssemblyManager> targets);
  }
}