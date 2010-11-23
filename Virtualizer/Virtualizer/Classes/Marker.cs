// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace MarkVirtual
{
  public class Marker
  {
    private CustomAttribute Att { get; set; }
    private StrongNameKeyPair DefaultKey { get; set; }
    private ICollection<StrongNameKeyPair> Keys { get; set; }

    public Marker (CustomAttribute nonVirtualAttribute, StrongNameKeyPair defaultKey, ICollection<StrongNameKeyPair> keys)
    {
      Att = nonVirtualAttribute;
      DefaultKey = defaultKey;
      Keys = keys;
    }

    private void SerializeModifications (AssemblyManager target, StrongNameKeyPair key)
    {
      SerializeModifications (new List<AssemblyManager>() { target }, key);
    }

    private void SerializeModifications (ICollection<AssemblyManager> targets, StrongNameKeyPair key)
    {
      WriterParameters param = new WriterParameters { StrongNameKeyPair = key };
      foreach (var assemblyManager in targets)
      {
        //Console.WriteLine ("serializeModifications write and set valid");
        assemblyManager.Assembly.Write (assemblyManager.FullPath + ".modif.dll", param);
        //assemblyManager.Assembly.Write (new MemoryStream(), param);
        assemblyManager.IsValid = true;
      }
    }

    public void OverrideMethods (ICollection<AssemblyManager> targets, Regex methodTargets, bool setVirtual)
    {
      foreach (var manager in targets)
      {
        foreach (var module in manager.Assembly.Modules)
        {
          foreach (var type in module.Types)
          {
            foreach (var method in type.Methods)
            {
              if (!methodTargets.IsMatch (method.FullName))
                continue;
              MarkMethod (method, Att, setVirtual);
              manager.IsValid = false;
            }
          }
        }
      }
    }

    public void MarkMethod (MethodDefinition target, CustomAttribute attribute, bool modify)
    {
      Console.WriteLine ("override: " + target.Name + " [mark virtual: " + modify + "], add/remove attribute: " + attribute.Constructor.FullName);
      if (modify)
      {
        if (!target.CustomAttributes.Contains(attribute))
          target.CustomAttributes.Add (attribute);
        if (!target.IsVirtual)
          target.IsVirtual = true;
      }
      else
      {
        if (target.CustomAttributes.Contains (attribute))
        {
          target.CustomAttributes.Remove (attribute);
          if (target.IsVirtual)
            target.IsVirtual = false;
        }
      }
    }

    public void Serialize (ICollection<AssemblyManager> targets)
    {
      if (targets == null || targets.Count <= 0)
        return;
      for (int k = 0; k < targets.Count; k++)
      {
        Console.WriteLine ("running... " + k + "  " + targets.ElementAt (k).Assembly.Name.Name);
        if (!targets.ElementAt (k).IsValid) {
          Console.WriteLine (targets.ElementAt (k).Assembly.Name.Name +  " invalid!");
          if (Serialize (targets.ElementAt (k)))
            k = -1;
        }
      }
    }

    private bool Serialize (AssemblyManager target)
    {
      Console.WriteLine ("serialize! " + target.Assembly.Name.Name);
          //bool isDirty = target.Dependencies.Aggregate (false, (current, mgr) => current | Serialize (mgr));
      foreach (var mgr in target.Dependencies)
        Serialize (mgr);
          //if (isDirty)
      Update (target);
      Console.WriteLine ("serialize: " + target.Assembly.Name.Name + " is " + (!target.IsValid ? "not valid -> serializeMod" : "valid!"));
      if (!target.IsValid)
      {  
        StrongNameKeyPair signKey = null;
        if (target.Assembly.Name.HasPublicKey &&            // shortcircuit is needed.
            (signKey = GetKey (target.Assembly.Name.PublicKey)) != null)
        {
          SerializeModifications (target, signKey);
        }
        else
        {
          target.KeyChanged = true;
          SerializeModifications (target, DefaultKey);
        }

          foreach (var mgr in target.References)
          {
            mgr.IsValid = false;
          }
        return true;
      }
      return false;
    }

    private void Update (AssemblyManager target)
    {
      Console.WriteLine (target.Assembly.Name.FullName);
      foreach (var mgr in target.Dependencies)
      {
        foreach (var module in target.Assembly.Modules)
        {
          for (int i = 0; i < module.AssemblyReferences.Count; i++)
          {
            bool sameName    = mgr.Equals (module.AssemblyReferences[i].Name);
            bool sameCulture = mgr.Assembly.Name.Culture == module.AssemblyReferences[i].Culture;
            bool sameVersion = mgr.Assembly.Name.Version == module.AssemblyReferences[i].Version;
            bool sameKey =  mgr.Assembly.Name.HasPublicKey && module.AssemblyReferences[i].PublicKeyToken!=null &&
                            mgr.Assembly.Name.PublicKeyToken.SequenceEqual( module.AssemblyReferences[i].PublicKeyToken);
            if (sameName && sameVersion && sameCulture)
            {
              if (!sameKey)
              {
                Console.WriteLine ("Update reference: " + module.AssemblyReferences[i].FullName + " ---> " + mgr.Assembly.Name.FullName);
                module.AssemblyReferences[i] = mgr.Assembly.Name;
              }
            }
          }
        }
      }
    }

    private StrongNameKeyPair GetKey (byte[] publicKey)
    {
      return Keys.FirstOrDefault (key => key.PublicKey.SequenceEqual (publicKey));
    }

  }
}