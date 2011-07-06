// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;

namespace AssemblyTransformer.FileSystem
{
  /// <summary>
  /// Abstraction of the filesystem. Useful for testing and generally exchanging the underlying filesystem.
  /// </summary>
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

    public bool FileExists (string fileName)
    {
      return File.Exists (fileName);
    }

    public AssemblyDefinition ReadAssembly (string fileName, ReaderParameters readerParameters)
    {
      return AssemblyDefinition.ReadAssembly (fileName, readerParameters);
    }

    public void WriteModuleDefinition (ModuleDefinition moduleDefinition, string fileName, WriterParameters writerParameters)
    {
      moduleDefinition.Write (fileName, writerParameters);
    }

    public Assembly LoadAssemblyFrom (string fileName)
    {
      return Assembly.LoadFrom (fileName);
    }

    public FileStream Open (string path, FileMode mode)
    {
      return File.Open (path, mode);
    }
  }
}