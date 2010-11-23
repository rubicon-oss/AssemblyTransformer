using System;
using System.Collections.Generic;

namespace MarkVirtual
{
  public interface IReferenceResolver
  {
    string WorkingDir { get; set; }

    ICollection<AssemblyManager> AssemblyManagers { get; }
  }
}
