// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;

namespace GenericAttributeGenerator
{
  public class GenericAttributeGenerator : IAssemblyTransformation
  {
    private readonly Type _genericMarker;

    public Type GenericMarker
    {
      get { return _genericMarker; }
    }

    public GenericAttributeGenerator (Type genericMarker)
    {
      _genericMarker = genericMarker;
    }

    public void Transform (IAssemblyTracker tracker)
    {
      foreach (var assembly in tracker.GetAssemblies())
      {
        foreach (var module in assembly.Modules)
        {
          foreach (var type in module.Types)
          {
            foreach (var attribute in type.CustomAttributes)
            {
              Console.WriteLine (attribute.AttributeType.FullName + " - " + _genericMarker.FullName);
              if (attribute.AttributeType.FullName == _genericMarker.FullName)
                tracker.MarkModified (assembly);
            }
          }
        }
      }
    }

    
  }
}