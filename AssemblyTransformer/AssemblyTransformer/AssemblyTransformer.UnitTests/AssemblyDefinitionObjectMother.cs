// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using Mono.Cecil;

namespace AssemblyTransformer.UnitTests
{
  public static class AssemblyDefinitionObjectMother
  {
    public static AssemblyDefinition CreateAssemblyDefinition ()
    {
      return AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("TestCase", null), "TestCase.dll", ModuleKind.Dll);
    }
  }
}