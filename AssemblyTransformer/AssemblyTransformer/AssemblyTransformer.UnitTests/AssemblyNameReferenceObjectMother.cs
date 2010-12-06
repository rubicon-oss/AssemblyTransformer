// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using Mono.Cecil;

namespace AssemblyTransformer.UnitTests
{
  public static class AssemblyNameReferenceObjectMother
  {
    public static readonly byte[] PublicKeyToken1 = new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 };
    public static readonly byte[] PublicKeyToken2 = new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x90 };
    public static readonly byte[] PublicKeyToken3 = new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x91 };
    public static readonly byte[] PublicKeyToken4 = new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x92 };
    public static readonly byte[] PublicKeyToken5 = new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x93 };

    public static readonly byte[] PublicKey1 = new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89, 0x00 };
    public static readonly byte[] PublicKey2 = new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x90, 0x00 };

    public static AssemblyNameReference CreateAssemblyNameReference (string shortName)
    {
      return new AssemblyNameReference (shortName, null);
    }

    public static AssemblyNameReference CreateAssemblyNameReferenceWithCulture (string shortName, string culture)
    {
      return new AssemblyNameReference (shortName, null) { Culture = culture };
    }

    public static AssemblyNameReference CreateAssemblyNameReferenceWithPublicKeyToken (string shortName, byte[] publicKeyToken)
    {
      return new AssemblyNameReference (shortName, null) { PublicKeyToken = publicKeyToken };
    }

    public static AssemblyNameReference CreateAssemblyNameReferenceWithPublicKey (string shortName, byte[] publicKey)
    {
      return new AssemblyNameReference (shortName, null) { PublicKey = publicKey, HasPublicKey = true};
    }

    public static AssemblyNameReference CreateAssemblyNameReferenceWithVersion (string shortName, string version)
    {
      return new AssemblyNameReference (shortName, new Version (version));
    }
  }
}