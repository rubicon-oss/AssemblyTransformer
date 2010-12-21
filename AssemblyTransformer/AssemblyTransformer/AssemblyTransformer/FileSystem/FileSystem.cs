// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace AssemblyTransformer.FileSystem
{
  public class FileSystem : IFileSystem
  {
    public IEnumerable<string> EnumerateFiles (string path, string searchPattern, SearchOption searchOption)
    {
      return Directory.EnumerateFiles (path, searchPattern, searchOption);
    }

    public void Move (string sourceFileName, string destFileName)
    {
      File.Move (sourceFileName, destFileName);
    }

    public AssemblyDefinition ReadAssembly (string fileName)
    {
      return AssemblyDefinition.ReadAssembly (fileName);
    }

    public void WriteModuleDefinition (ModuleDefinition moduleDefinition, string fileName, WriterParameters writerParameters)
    {
      moduleDefinition.Write (fileName, writerParameters);
    }

    public FileStream Open (string path, FileMode mode)
    {
      return File.Open (path, mode);
    }
  }
}