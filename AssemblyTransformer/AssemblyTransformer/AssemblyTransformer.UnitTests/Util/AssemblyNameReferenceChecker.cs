// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using Mono.Cecil;
using NUnit.Framework;

namespace AssemblyTransformer.UnitTests
{
  public static class AssemblyNameReferenceChecker
  {
    public static void CheckNameReferences (AssemblyNameReference expectedReference, AssemblyNameReference actualReference)
    {
      Assert.That (actualReference.Attributes, Is.EqualTo (expectedReference.Attributes));
      Assert.That (actualReference.Culture, Is.EqualTo (expectedReference.Culture));
      Assert.That (actualReference.FullName, Is.EqualTo (expectedReference.FullName));
      Assert.That (actualReference.Hash, Is.EqualTo (expectedReference.Hash));
      Assert.That (actualReference.HashAlgorithm, Is.EqualTo (expectedReference.HashAlgorithm));
      Assert.That (actualReference.HasPublicKey, Is.EqualTo (expectedReference.HasPublicKey));
      Assert.That (actualReference.IsRetargetable, Is.EqualTo (expectedReference.IsRetargetable));
      Assert.That (actualReference.IsSideBySideCompatible, Is.EqualTo (expectedReference.IsSideBySideCompatible));
      Assert.That (actualReference.MetadataScopeType, Is.EqualTo (expectedReference.MetadataScopeType));
      Assert.That (actualReference.MetadataToken, Is.EqualTo (expectedReference.MetadataToken));
      Assert.That (actualReference.Name, Is.EqualTo (expectedReference.Name));
      Assert.That (actualReference.PublicKey, Is.EqualTo (expectedReference.PublicKey));
      Assert.That (actualReference.PublicKeyToken, Is.EqualTo (expectedReference.PublicKeyToken));
      Assert.That (actualReference.Version, Is.EqualTo (expectedReference.Version));
    }
  }
}