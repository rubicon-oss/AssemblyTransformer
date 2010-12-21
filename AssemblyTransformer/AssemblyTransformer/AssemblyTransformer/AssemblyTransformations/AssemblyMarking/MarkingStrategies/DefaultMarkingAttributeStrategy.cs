using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace AssemblyTransformer.AssemblyTransformations.AssemblyMarking.MarkingStrategies
{
  public class DefaultMarkingAttributeStrategy : MarkingAttributeStrategy
  {

    public DefaultMarkingAttributeStrategy (string defAttributeNamespace, string defAttributeName)
            : base (defAttributeNamespace, defAttributeName) {}

    protected override TypeDefinition CreateCustomAttributeType (ModuleDefinition targetModule)
    {
      var customType = new TypeDefinition (_attributeNamespace,
                                           _attributeName,
                                           TypeAttributes.Public | TypeAttributes.Class,
                                           targetModule.Import (typeof (Attribute)));
      var ctor = new MethodDefinition (
          ".ctor",
          MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
          targetModule.Import (typeof (void)));
      ctor.HasThis = true;

      var il = ctor.Body.GetILProcessor ();
      il.Emit (OpCodes.Ldarg_0);
      il.Emit (
          OpCodes.Call,
          targetModule.Import (typeof (Attribute).GetConstructor (BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null)));
      il.Emit (OpCodes.Ret);

      customType.Methods.Add (ctor);

      return customType;
    }

    public override void AddCustomAttribute (MethodDefinition methodDefinition, AssemblyDefinition assemblyOfMethod)
    {
      AddCustomAttribute (methodDefinition, assemblyOfMethod.MainModule);
    }
  }
}