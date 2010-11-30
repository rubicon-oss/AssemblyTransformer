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
      return CreateAssemblyDefinition ("TestCase");
    }

    public static AssemblyDefinition CreateAssemblyDefinition (string name)
    {
      return CreateAssemblyDefinition (name, null);
    }

    public static AssemblyDefinition CreateAssemblyDefinition (string name, string culture)
    {
      return AssemblyDefinition.CreateAssembly (new AssemblyNameDefinition (name, null) { Culture = culture }, name + ".dll", ModuleKind.Dll); 
    }

  }
}