// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyMethodsVirtualizer.MarkingStrategies;
using AssemblyMethodsVirtualizer.TargetSelection;
using AssemblyTransformer;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;
using AssemblyTransformer.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace AssemblyMethodsVirtualizer
{
  /// <summary>
  /// This is an implementation of an assembly transformation. 
  /// The transformation's goal is, to mark the targeted methods in the given assemblytracker's assemblies
  /// as virtual. Also the given marking strategy is executed on every target.
  /// The targets are: all methods in the given assembly whose full name matches the given regular expression.
  /// After a method has been set to virtual, the whole corresponding assembly is marked as modified for later
  /// handling.
  /// </summary>
  public class AssemblyMethodsVirtualizer : IAssemblyTransformation
  {
    private readonly HashSet<string> _modMethods = new HashSet<string> (); 

    private readonly IMarkingAttributeStrategy _markingAttributeStrategy;
    private readonly ITargetSelectionFactory _selectionFactory;
    private ITargetSelectionStrategy _selectionStrategy;

    public IMarkingAttributeStrategy MarkingAttributeStrategy
    {
      get { return _markingAttributeStrategy; }
    }

    public ITargetSelectionStrategy SelectionStrategy
    {
      get { return _selectionStrategy; }
    }

    public AssemblyMethodsVirtualizer (IMarkingAttributeStrategy markingAttributeStrategy, ITargetSelectionFactory targetSelectionFactory)
    {
      ArgumentUtility.CheckNotNull ("markingAttributeStrategy", markingAttributeStrategy);
      ArgumentUtility.CheckNotNull ("targetSelectionFactory", targetSelectionFactory);

      _markingAttributeStrategy = markingAttributeStrategy;
      _selectionFactory = targetSelectionFactory;
    }

    public void Transform (IAssemblyTracker tracker)
    {
      ArgumentUtility.CheckNotNull ("tracker", tracker);
      _selectionStrategy = _selectionFactory.CreateSelector (tracker);

      var modifiedMethods =  from assemblyDefinition in tracker.GetAssemblies()
                             from typeDefinition in assemblyDefinition.LoadAllTypes ()
                             from methodDefinition in typeDefinition.Methods
                             where _selectionStrategy.IsTarget (methodDefinition, assemblyDefinition)
                             select new { Assembly = assemblyDefinition, Method = methodDefinition };

      foreach (var modifiedMethodDefinition in modifiedMethods.ToList())
      {
        if (modifiedMethodDefinition.Method.IsVirtual)
        {
          modifiedMethodDefinition.Method.IsFinal = false;
          modifiedMethodDefinition.Method.IsCheckAccessOnOverride = false;
        }
        if (!modifiedMethodDefinition.Method.IsVirtual      && 
            !modifiedMethodDefinition.Method.IsStatic       && 
            !modifiedMethodDefinition.Method.IsConstructor  &&
            !modifiedMethodDefinition.Method.CustomAttributes.Any(ca => ca.AttributeType.Namespace == "System.Runtime.Serialization"))
        {
          tracker.MarkModified (modifiedMethodDefinition.Assembly);
          modifiedMethodDefinition.Method.IsVirtual = true;
          modifiedMethodDefinition.Method.IsNewSlot = true;
          _markingAttributeStrategy.AddCustomAttribute (modifiedMethodDefinition.Method, modifiedMethodDefinition.Assembly);
          
          if (!modifiedMethodDefinition.Method.DeclaringType.IsValueType)
            _modMethods.Add (modifiedMethodDefinition.Method.FullName);
        }
      }

      foreach (var modifiedAssembly in tracker.GetModifiedAssemblies ())
        foreach (var modifiedType in modifiedAssembly.LoadAllTypes ())
          foreach (var modifiedMethod in modifiedType.Methods)
            ReplaceNonVirtualILcode (modifiedMethod);
    }

    private void ReplaceNonVirtualILcode (MethodDefinition modifiedMethod)
    {
      if (!modifiedMethod.HasBody)
        return;

      modifiedMethod.Body.SimplifyMacros();

      var instruction = modifiedMethod.Body.Instructions;
      for (int i = 0; i < instruction.Count; i++)
      {
        if (instruction[i].OpCode == OpCodes.Call)
        {
          var call = instruction[i].Operand;
          while (call is MethodSpecification)     
            call = ((MethodSpecification) instruction[i].Operand).ElementMethod;

          if (_modMethods.Contains (((MethodReference) call).FullName))
            instruction[i].OpCode = OpCodes.Callvirt;
        }
        else if (instruction[i].OpCode == OpCodes.Ldftn)
        {
          var load = instruction[i].Operand;
          while (load is MethodSpecification)
            load = ((MethodSpecification) instruction[i].Operand).ElementMethod;

          if (_modMethods.Contains (((MethodReference) load).FullName))
          {
            instruction.Insert (i, Instruction.Create (OpCodes.Dup));
            ++i;
            instruction[i].OpCode = OpCodes.Ldvirtftn;
          }
        }
      }

      modifiedMethod.Body.OptimizeMacros();
    }
  }
}