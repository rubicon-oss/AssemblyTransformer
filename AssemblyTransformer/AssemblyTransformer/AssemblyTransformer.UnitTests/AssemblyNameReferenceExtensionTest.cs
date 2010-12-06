using System;
using Mono.Cecil;
using NUnit.Framework;

namespace AssemblyTransformer.UnitTests
{
  [TestFixture]
  public class AssemblyNameReferenceExtensionTest
  {
    [Test]
    public void ReferenceMatchesDefinition_True_ShortNameOnly ()
    {
      var reference = AssemblyNameReferenceObjectMother.CreateAssemblyNameReference ("NameOnly");
      var definitionWithShortNameOnly = AssemblyNameReferenceObjectMother.CreateAssemblyNameReference ("NameOnly");
      var definitionWithCulture = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithCulture ("NameOnly", "de");
      var definitionWithVersion = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithVersion ("NameOnly", "2.0");
      var definitionWithPublicKeyToken = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithPublicKeyToken (
          "NameOnly", AssemblyNameReferenceObjectMother.PublicKeyToken1);
      var definitionWithPublicKey = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithPublicKey (
          "NameOnly", AssemblyNameReferenceObjectMother.PublicKey1);

      Assert.That (reference.MatchesDefinition (definitionWithShortNameOnly), Is.True);
      Assert.That (reference.MatchesDefinition (definitionWithCulture), Is.True);
      Assert.That (reference.MatchesDefinition (definitionWithVersion), Is.True);
      Assert.That (reference.MatchesDefinition (definitionWithPublicKeyToken), Is.True);
      Assert.That (reference.MatchesDefinition (definitionWithPublicKey), Is.True);
    }

    [Test]
    public void ReferenceMatchesDefinition_False_ShortNameDiffers ()
    {
      var reference = AssemblyNameReferenceObjectMother.CreateAssemblyNameReference ("NameOnly");
      var definitionWithShortNameOnly = AssemblyNameReferenceObjectMother.CreateAssemblyNameReference ("NameOnlyDifferent");

      Assert.That (reference.MatchesDefinition (definitionWithShortNameOnly), Is.False);
    }

    [Test]
    public void ReferenceMatchesDefinition_False_CultureDiffers ()
    {
      var referenceWithCulture = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithCulture ("NameOnly", "en");
      var definitionWithCulture = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithCulture ("NameOnly", "de");
      var definitionWithoutCulture = AssemblyNameReferenceObjectMother.CreateAssemblyNameReference ("NameOnly");

      Assert.That (referenceWithCulture.MatchesDefinition (definitionWithCulture), Is.False);
      Assert.That (referenceWithCulture.MatchesDefinition (definitionWithoutCulture), Is.False);
    }

    [Test]
    public void ReferenceMatchesDefinition_False_VersionDiffers ()
    {
      var referenceWithVersion = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithVersion ("NameOnly", "1.0");
      var definitionWithVersion = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithVersion ("NameOnly", "2.0");
      var definitionWithoutVersion = AssemblyNameReferenceObjectMother.CreateAssemblyNameReference ("NameOnly");

      Assert.That (referenceWithVersion.MatchesDefinition (definitionWithVersion), Is.False);
      Assert.That (referenceWithVersion.MatchesDefinition (definitionWithoutVersion), Is.False);
    }

    [Test]
    public void ReferenceMatchesDefinition_False_PublicKeyTokenDiffers ()
    {
      var referenceWithPublicKeyToken = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithPublicKeyToken (
          "NameOnly", AssemblyNameReferenceObjectMother.PublicKeyToken1);
      var definitionWithPublicKeyToken = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithPublicKeyToken (
          "NameOnly", AssemblyNameReferenceObjectMother.PublicKeyToken2);
      var definitionWithoutPublicKeyToken = AssemblyNameReferenceObjectMother.CreateAssemblyNameReference ("NameOnly");

      Assert.That (referenceWithPublicKeyToken.MatchesDefinition (definitionWithPublicKeyToken), Is.False);
      Assert.That (referenceWithPublicKeyToken.MatchesDefinition (definitionWithoutPublicKeyToken), Is.False);
    }

    [Test]
    public void ReferenceMatchesDefinition_False_PublicKeyDiffers ()
    {
      var referenceWithPublicKey = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithPublicKey (
          "NameOnly", AssemblyNameReferenceObjectMother.PublicKey1);
      var definitionWithPublicKey = AssemblyNameReferenceObjectMother.CreateAssemblyNameReferenceWithPublicKey (
          "NameOnly", AssemblyNameReferenceObjectMother.PublicKey2);
      var definitionWithoutPublicKey = AssemblyNameReferenceObjectMother.CreateAssemblyNameReference ("NameOnly");

      Assert.That (referenceWithPublicKey.MatchesDefinition (definitionWithPublicKey), Is.False);
      Assert.That (referenceWithPublicKey.MatchesDefinition (definitionWithoutPublicKey), Is.False);
    }

    [Test]
    public void Clone_WithAllSet ()
    {
      var referenceWithAllSet = AssemblyNameReferenceObjectMother.CreateAssemblyNameReference ("AllSet");
      referenceWithAllSet.Culture = "de";
      referenceWithAllSet.Hash = new byte[0];
      referenceWithAllSet.HashAlgorithm = AssemblyHashAlgorithm.SHA1;
      referenceWithAllSet.IsRetargetable = true;
      referenceWithAllSet.IsSideBySideCompatible = true;
      referenceWithAllSet.MetadataToken = new MetadataToken(TokenType.Module, 1);
      referenceWithAllSet.PublicKey = AssemblyNameReferenceObjectMother.PublicKey1;
      referenceWithAllSet.Version = new Version("2.0.0.0");
      var copyReference = referenceWithAllSet.Clone();

      AssemblyNameReferenceChecker.CheckNameReferences (referenceWithAllSet, copyReference);

      Assert.That (copyReference.MetadataToken, Is.Not.SameAs (referenceWithAllSet.MetadataToken));
      Assert.That (copyReference.PublicKey, Is.Not.SameAs (referenceWithAllSet.PublicKey));
      Assert.That (copyReference.PublicKeyToken, Is.Not.SameAs (referenceWithAllSet.PublicKeyToken));
      Assert.That (copyReference.Version, Is.Not.SameAs (referenceWithAllSet.Version));
    }

    [Test]
    public void Clone_WithNoneSet ()
    {
      var referenceWithNoneSet = AssemblyNameReferenceObjectMother.CreateAssemblyNameReference ("NothingSet");
      var copyReference = referenceWithNoneSet.Clone ();

      AssemblyNameReferenceChecker.CheckNameReferences(referenceWithNoneSet, copyReference);
      Assert.That (copyReference.MetadataToken, Is.Not.SameAs (referenceWithNoneSet.MetadataToken));
    }
  }
}