// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.Extensions;
using Mono.Cecil;

namespace AssemblyTransformer.AppDomainBroker
{
  public static class CecilResolver
  {
    private static Dictionary<MemberID, Tuple<MethodDefinition, AssemblyNameReference>> _cachedMethods =
      new Dictionary<MemberID, Tuple<MethodDefinition, AssemblyNameReference>> ();
    private static Dictionary<MemberID, Tuple<TypeDefinition, AssemblyNameReference>> _cachedTypes = 
      new Dictionary<MemberID, Tuple<TypeDefinition, AssemblyNameReference>> ();

    public static MemberID CreateMemberID (TypeReference type)
    {
      return new MemberID (type.Module.Assembly.Name.BuildReflectionAssemblyQualifiedName (type),
                            type.Module.FullyQualifiedName, type.MetadataToken.ToInt32 ()
                            );
    }

    public static MemberID CreateMemberID (MethodReference method)
    {
      return new MemberID (method.Module.Assembly.Name.BuildReflectionAssemblyQualifiedName (method.DeclaringType),
                            method.Module.FullyQualifiedName, method.MetadataToken.ToInt32 ()
                            );
    }


    /// <summary>
    /// Resolves the given memberID to a monocecil typeDefinition. The out parameter is null if the assembly containing the type is an untracked assembly
    /// and does not need to be tracked. In case it is tracked, the out parameter contains the containing assembly and a new reference has to be added.
    /// </summary>
    public static TypeDefinition ResolveToTypeDefinition (MemberID member, IAssemblyTracker tracker, out AssemblyNameReference containingTrackedAssembly)
    {
      if (member == null)
      {
        containingTrackedAssembly = null;
        return null;
      }
      containingTrackedAssembly = null;
      if (!_cachedTypes.ContainsKey (member))
      {
        AssemblyDefinition containingAssembly = null;
        foreach (var assemblyDefinition in tracker.GetAssemblies())
          if (assemblyDefinition.FullName == member.AssemblyQualifiedTypeName.Substring (member.AssemblyQualifiedTypeName.IndexOf (",") + 1).Trim())
            containingAssembly = assemblyDefinition;

        if (containingAssembly == null)
          containingAssembly = AssemblyDefinition.ReadAssembly (member.ModuleName);
        else
          containingTrackedAssembly = containingAssembly.Name;

        foreach (var moduleDefinition in containingAssembly.Modules)
        {
          if (moduleDefinition.FullyQualifiedName == member.ModuleName)
            foreach (var typeDefinition in moduleDefinition.Types)
              if (typeDefinition.FullName == member.AssemblyQualifiedTypeName.Substring (0, member.AssemblyQualifiedTypeName.IndexOf (",")))
              {
                _cachedTypes[member] = new Tuple<TypeDefinition, AssemblyNameReference> (typeDefinition, containingAssembly.Name);
                return _cachedTypes[member].Item1;
              }
        }
        containingTrackedAssembly = null;
        return null;
      }
      containingTrackedAssembly = _cachedTypes[member].Item2;
      return _cachedTypes[member].Item1;
    }

    /// <summary>
    /// Resolves the given memberID to a monocecil methodDefinition. The out parameter is null if the assembly containing the method is an untracked assembly
    /// and does not need to be tracked. In case it is tracked, the out parameter contains the containing assembly and a new reference has to be added.
    /// </summary>
    public static MethodDefinition ResolveToMethodDefinition (MemberID member, IAssemblyTracker tracker, out AssemblyNameReference containingTrackedAssembly)
    {
      if (member == null)
      {
        containingTrackedAssembly = null;
        return null;
      }
      if (!_cachedMethods.ContainsKey (member))
      {
        var containingType = ResolveToTypeDefinition (member, tracker, out containingTrackedAssembly);
        if (containingType == null)
          return null;

        foreach (var md in containingType.Methods)
          if (md.MetadataToken.ToInt32() == member.Token)
          {
            _cachedMethods[member] = new Tuple<MethodDefinition, AssemblyNameReference>(md, containingTrackedAssembly);
            return _cachedMethods[member].Item1;
          }
        containingTrackedAssembly = null;
        return null;
      }
      containingTrackedAssembly = _cachedMethods[member].Item2;
      return _cachedMethods[member].Item1;
    }
  }
}