// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AssemblyTransformer;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;
using AssemblyTransformer.Extensions;
using MethodVirtualizer.ILCodeGeneration;
using MethodVirtualizer.InfoProvider;
using Mono.Cecil;

namespace MethodVirtualizer
{
  public class MethodVirtualizer : IAssemblyTransformation
  {
    private IMethodVirtualizerInfoWrapper _wrapper;
    private readonly ICodeGenerator _codeGenerator;

    public MethodVirtualizer (IMethodVirtualizerInfoWrapper wrapper, ICodeGenerator codeGenerator)
    {
      ArgumentUtility.CheckNotNull ("wrapper", wrapper);
      ArgumentUtility.CheckNotNull ("codeGenerator", codeGenerator);
      _wrapper = wrapper;
      _codeGenerator = codeGenerator;
    }

    public void Transform (IAssemblyTracker tracker)
    {
      ArgumentUtility.CheckNotNull ("tracker", tracker);
      MethodDefinition customAttributeCtor;
      string virtualMethodName;

      var modifiedMethods = from assemblyDefinition in tracker.GetAssemblies ()
                            from typeDefinition in assemblyDefinition.LoadAllTypes ()
                            where _wrapper.ShouldVirtualizeType (typeDefinition)
                            from methodDefinition in typeDefinition.Methods
                            where _wrapper.ShouldVirtualizeMethod (methodDefinition)
                            select new { Assembly = assemblyDefinition, Method = methodDefinition };

      foreach (var modifiedMethodDefinition in modifiedMethods.ToList ())
      {
        if (!modifiedMethodDefinition.Method.IsVirtual &&
            !modifiedMethodDefinition.Method.IsStatic &&
            !modifiedMethodDefinition.Method.IsConstructor &&
            !modifiedMethodDefinition.Method.CustomAttributes.Any (ca => ca.AttributeType.Namespace == "System.Runtime.Serialization"))
        {
          tracker.MarkModified (modifiedMethodDefinition.Assembly);

          var virtualMethod = _codeGenerator.GenerateMethodAndMoveBody (
            modifiedMethodDefinition.Method, 
            (virtualMethodName = _wrapper.GetUnspeakableMethodName (modifiedMethodDefinition.Method))
            );
          modifiedMethodDefinition.Method.DeclaringType.Methods.Add (virtualMethod);

          if ((customAttributeCtor = _wrapper.GetVirtualizedAttribute (modifiedMethodDefinition.Assembly, tracker)) != null)
            AddAttributes (modifiedMethodDefinition.Method, virtualMethod, customAttributeCtor);
        }
      }
    }

    private void AddAttributes (MethodDefinition originalMethod, MethodDefinition virtualMethod, MethodDefinition customAttributeCtor)
    {
      var compilerGeneratedCtor = virtualMethod.DeclaringType.Module.Import (typeof (CompilerGeneratedAttribute).GetConstructor (Type.EmptyTypes));
      if (!virtualMethod.CustomAttributes.Any (att => att.Constructor.FullName == compilerGeneratedCtor.FullName))
        virtualMethod.CustomAttributes.Add (new CustomAttribute (compilerGeneratedCtor));

      var editorBrowsableCtor = virtualMethod.DeclaringType.Module.Import (
        typeof (EditorBrowsableAttribute).GetConstructor (new[] { typeof (EditorBrowsableState) }));
      var editorBrowsableAtt = new CustomAttribute (editorBrowsableCtor);
      editorBrowsableAtt.ConstructorArguments.Add (
        new CustomAttributeArgument (virtualMethod.DeclaringType.Module.Import (typeof (EditorBrowsableState)), EditorBrowsableState.Never));
      if (!virtualMethod.CustomAttributes.Any (att => att.Constructor.FullName == editorBrowsableCtor.FullName))
        virtualMethod.CustomAttributes.Add (editorBrowsableAtt);
      
      if (customAttributeCtor == null)
        return;

      var referringAttributeCtor = originalMethod.DeclaringType.Module.Import (customAttributeCtor);
      if (!originalMethod.CustomAttributes.Any (att => att.Constructor.FullName == referringAttributeCtor.FullName))
      {
        var referringAttribute = new CustomAttribute(referringAttributeCtor);
        referringAttribute.ConstructorArguments.Add (new CustomAttributeArgument (originalMethod.DeclaringType.Module.TypeSystem.String, virtualMethod.Name));
        originalMethod.CustomAttributes.Add (referringAttribute);
      }
    }
  }
}