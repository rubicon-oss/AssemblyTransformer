// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

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

    public static AssemblyDefinition CreateMultiModuleAssemblyDefinition ()
    {
      AssemblyDefinition corlib = AssemblyDefinition.ReadAssembly (typeof (object).Module.FullyQualifiedName);
      
      AssemblyDefinition assemblyDefinition = AssemblyDefinitionObjectMother.CreateAssemblyDefinition ("TestAssembly");
      assemblyDefinition.MainModule.AssemblyReferences.Add (corlib.Name);
      assemblyDefinition.Name.Version = new Version ("1.0.0.0");

      ModuleDefinition secondModule = ModuleDefinition.CreateModule ("TestSecondModule.netmodule", ModuleKind.NetModule);
      TypeReference typereference = secondModule.Import (typeof (void));
      TypeReference typereferenceObject = secondModule.Import (typeof (object));


      TypeDefinition type = new TypeDefinition ("TestSpace", "TestType", TypeAttributes.Public | TypeAttributes.Class, assemblyDefinition.MainModule.Import(typeof(object)));
      MethodDefinition method = new MethodDefinition ("TestMethod", MethodAttributes.Public | MethodAttributes.HideBySig, typereference);
      var il = method.Body.GetILProcessor ();
      il.Emit (OpCodes.Ret);

      type.Methods.Add (method);
      method.DeclaringType = type;
      assemblyDefinition.MainModule.Types.Add (type);

      
      TypeDefinition secondType = new TypeDefinition ("TestSpace", "TestSecondType", TypeAttributes.Public | TypeAttributes.Class, secondModule.Import(typeof(object)));
      MethodDefinition secondMethod = new MethodDefinition ("TestSecondMethod", MethodAttributes.Public | MethodAttributes.HideBySig, typereference);
      il = secondMethod.Body.GetILProcessor ();
      il.Emit (OpCodes.Ret);

      secondType.Methods.Add (secondMethod);
      secondModule.Types.Add (secondType);
      secondMethod.DeclaringType = secondType;
      assemblyDefinition.Modules.Add (secondModule);
      assemblyDefinition.MainModule.ModuleReferences.Add (secondModule);
      
      ExportedType e = new ExportedType ("TestSpace", "TestSecondType", new ModuleReference (secondModule.Name));
      assemblyDefinition.MainModule.ExportedTypes.Add (e);

      var testingDirectory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, @"temp\testing");
      if (!Directory.Exists (testingDirectory))
        Directory.CreateDirectory (testingDirectory);

      assemblyDefinition.Write (Path.Combine (testingDirectory, "test.mainmodule"));
      secondModule.Write (Path.Combine (testingDirectory, "test.module"));
      assemblyDefinition = AssemblyDefinition.ReadAssembly (Path.Combine (testingDirectory, "test.mainmodule"));

      return assemblyDefinition;
    }

    public static AssemblyDefinition CreateSignedMultiModuleAssemblyDefinition ()
    {
      var assembly = CreateMultiModuleAssemblyDefinition();
      assembly.Name.HashAlgorithm = AssemblyHashAlgorithm.SHA1;
      assembly.Name.HasPublicKey = true;
      assembly.Name.PublicKey = AssemblyNameReferenceObjectMother.RealKeyPair().PublicKey;
      assembly.MainModule.Attributes |= ModuleAttributes.StrongNameSigned;
      assembly.Modules[1].Attributes |= ModuleAttributes.StrongNameSigned;
      return assembly;
    }
  }
}