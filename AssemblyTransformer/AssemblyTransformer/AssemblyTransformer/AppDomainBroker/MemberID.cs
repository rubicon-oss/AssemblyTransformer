// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Runtime.Serialization;
using Mono.Cecil;

namespace AssemblyTransformer.AppDomainBroker
{
  [Serializable]
  public class MemberID : ISerializable
  {
    public string AssemblyQualifiedTypeName;
    public string ModuleName;
    public int Token;

    public MemberID ()
    {
      Console.WriteLine ("Instantiated in " + this.GetType().Assembly.CodeBase + " in AppDomain " + AppDomain.CurrentDomain.FriendlyName);
    }

    public MemberID (string assemblyName, string moduleName, MetadataToken token)
    {
      AssemblyQualifiedTypeName = assemblyName;
      ModuleName = moduleName;
      Token = token.ToInt32();
    }

    public MemberID (string assemblyName, string moduleName, int token)
    {
      AssemblyQualifiedTypeName = assemblyName;
      ModuleName = moduleName;
      Token = token;
    }

    public MemberID (SerializationInfo info, StreamingContext context)
    {
      AssemblyQualifiedTypeName = (string) info.GetValue ("AssemblyQualifiedTypeName", typeof (string));
      ModuleName = (string) info.GetValue ("ModuleName", typeof (string));
      Token = (int) info.GetValue ("Token", typeof (int));
    }

    public void GetObjectData (SerializationInfo info, StreamingContext context)
    {
      info.AddValue ("AssemblyQualifiedTypeName", AssemblyQualifiedTypeName);
      info.AddValue ("ModuleName", ModuleName);
      info.AddValue ("Token", Token);
    }
  }
}