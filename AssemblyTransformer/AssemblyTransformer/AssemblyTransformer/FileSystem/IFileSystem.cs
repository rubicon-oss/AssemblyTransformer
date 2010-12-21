using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace AssemblyTransformer.FileSystem
{
  public interface IFileSystem
  {
    IEnumerable<string> EnumerateFiles (string path, string searchPattern, SearchOption searchOption);
    void Move (string sourceFileName, string destFileName);

    AssemblyDefinition ReadAssembly (string fileName);
    void WriteModuleDefinition (ModuleDefinition moduleDefinition, string fileName, WriterParameters writerParameters);

    FileStream Open (string path, FileMode mode);
  }
}