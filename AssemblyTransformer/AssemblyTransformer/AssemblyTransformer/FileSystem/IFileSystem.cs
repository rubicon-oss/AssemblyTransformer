using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace AssemblyTransformer.FileSystem
{
  /// <summary>
  /// Interface for filesystem abstraction.
  /// </summary>
  public interface IFileSystem
  {
    IEnumerable<string> EnumerateFiles (string path, string searchPattern, SearchOption searchOption);
    void Move (string sourceFileName, string destFileName);
    FileStream Open (string path, FileMode mode);
    bool FileExists (string fileName);

    AssemblyDefinition ReadAssembly (string fileName);
    void WriteModuleDefinition (ModuleDefinition moduleDefinition, string fileName, WriterParameters writerParameters);
  }
}