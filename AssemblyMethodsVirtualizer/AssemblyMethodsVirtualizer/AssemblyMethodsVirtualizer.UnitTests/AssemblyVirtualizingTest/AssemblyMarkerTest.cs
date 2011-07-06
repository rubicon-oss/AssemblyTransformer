// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using AssemblyMethodsVirtualizer.ILCodeGeneration;
using AssemblyMethodsVirtualizer.MarkingStrategies;
using AssemblyMethodsVirtualizer.TargetSelection;
using AssemblyTransformer;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.TypeDefinitionCaching;
using Mono.Cecil;
using NUnit.Framework;
using Rhino.Mocks;

namespace AssemblyMethodsVirtualizer.UnitTests.AssemblyVirtualizingTest
{
  [TestFixture]
  public class AssemblyMarkerTest
  {
    private IAssemblyTracker _tracker;
    private AssemblyDefinition _assemblyDefinition;

    private AssemblyMethodsVirtualizer _methodsVirtualizer;

    private IMarkingAttributeStrategy _markingAttributeStrategy;
    private ITargetSelectionFactory _selectionFactory;
    private ICodeGenerator _codeGenerator;
    private OptionSet _options;

    [SetUp]
    public void SetUp ()
    {
      _assemblyDefinition = AssemblyDefinitionObjectMother.CreateMultiModuleAssemblyDefinition();
      _tracker = new AssemblyTracker (new [] { _assemblyDefinition }, new TypeDefinitionCache());
      _markingAttributeStrategy = MockRepository.GenerateStub<IMarkingAttributeStrategy> ();
      _codeGenerator = MockRepository.GenerateStub<ICodeGenerator>();
      _selectionFactory = new TargetSelectorFactory ();
      _options = new OptionSet ();
      _methodsVirtualizer = new AssemblyMethodsVirtualizer (_markingAttributeStrategy, _selectionFactory, _codeGenerator);
    }

    [Test]
    public void OverrideMethods_MarksAssemblyWithMatchingMethodsModified ()
    {
      _selectionFactory.AddOptions (_options);
      _options.Parse (new[] { "--regex=(.*)" });
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];
      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);
      _codeGenerator.Expect (s => s.GenerateMethodAndMoveBody (Arg<MethodDefinition>.Is.Anything)).
          Return (new MethodDefinition ("GeneratedMethod", MethodAttributes.Public | MethodAttributes.Virtual, methodMain.ReturnType));
      _codeGenerator.Expect (s => s.GenerateMethodAndMoveBody (Arg<MethodDefinition>.Is.Anything)).
          Return (new MethodDefinition ("GeneratedMethod", MethodAttributes.Public | MethodAttributes.Virtual, methodMain.ReturnType));

      _methodsVirtualizer.Transform (_tracker);

      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.True);
    }

    [Test]
    public void OverrideMethods_DoesNotMarkAssemblyWithougMatchingMethodsModified ()
    {
      _selectionFactory.AddOptions (_options);
      _options.Parse (new[] { "--regex=xxx" });
      var _methodsVirtualizerNoMatch = new AssemblyMethodsVirtualizer (_markingAttributeStrategy, _selectionFactory, _codeGenerator);
      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);

      _methodsVirtualizerNoMatch.Transform (_tracker);

      Assert.That (_tracker.IsModified (_assemblyDefinition), Is.False);
    }

    [Test]
    public void OverrideMethods_DoesNotSetNonMatchingMethodVirtual ()
    {
      _selectionFactory.AddOptions (_options);
      _options.Parse (new[] { "--regex=TestMethodDoesnotMatch" });
      _methodsVirtualizer = new AssemblyMethodsVirtualizer (_markingAttributeStrategy, _selectionFactory, _codeGenerator);
      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.False);

      _methodsVirtualizer.Transform (_tracker);

      Assert.That (_assemblyDefinition.MainModule.Types[1].Methods[0].IsVirtual, Is.False);
    }

    [Test]
    public void OverrideMethods_BothMethodsMarked ()
    {
      _selectionFactory.AddOptions (_options);
      _options.Parse (new[] { "--regex=(.*)" });
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];
      MethodDefinition methodModule = _assemblyDefinition.Modules[1].Types[1].Methods[0];
      _codeGenerator.Expect (s => s.GenerateMethodAndMoveBody (methodMain)).
          Return (new MethodDefinition ("GeneratedMethod", MethodAttributes.Public | MethodAttributes.Virtual, methodMain.ReturnType));
      _codeGenerator.Expect (s => s.GenerateMethodAndMoveBody (methodModule)).
          Return (new MethodDefinition ("GeneratedMethod", MethodAttributes.Public | MethodAttributes.Virtual, methodModule.ReturnType));

      _methodsVirtualizer.Transform (_tracker);

      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodMain, _assemblyDefinition));
      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodModule, _assemblyDefinition));
      _codeGenerator.AssertWasCalled (s => s.GenerateMethodAndMoveBody (methodModule));
      _codeGenerator.AssertWasCalled (s => s.GenerateMethodAndMoveBody (methodMain));
    }

    [Test]
    public void OverrideMethods_MainModule_MethodMarked()
    {
      _selectionFactory.AddOptions (_options);
      _options.Parse (new[] { "--regex=TestMethod" });
      _methodsVirtualizer = new AssemblyMethodsVirtualizer (_markingAttributeStrategy, _selectionFactory, _codeGenerator);
      MethodDefinition methodMain = _assemblyDefinition.MainModule.Types[1].Methods[0];
      _codeGenerator.Expect (s => s.GenerateMethodAndMoveBody (methodMain)).
          Return (new MethodDefinition ("GeneratedMethod", MethodAttributes.Public | MethodAttributes.Virtual, methodMain.ReturnType));

      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));

      _methodsVirtualizer.Transform (_tracker);

      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodMain, _assemblyDefinition));
      _codeGenerator.AssertWasCalled (s => s.GenerateMethodAndMoveBody (methodMain));
    }

    [Test]
    public void OverrideMethods_SecondaryModule_MethodMarked ()
    {
      _selectionFactory.AddOptions (_options);
      _options.Parse (new[] { "--regex=TestSecondMethod" });
      _methodsVirtualizer = new AssemblyMethodsVirtualizer (_markingAttributeStrategy, _selectionFactory, _codeGenerator);
      MethodDefinition methodModule = _assemblyDefinition.Modules[1].Types[1].Methods[0];
      Assert.That (_assemblyDefinition.MainModule.Types[1].CustomAttributes, Is.Empty);
      Assert.That (_assemblyDefinition.Modules.Count, Is.EqualTo (2));
      Assert.That (_assemblyDefinition.MainModule.Types.Count, Is.EqualTo (2));
      _codeGenerator.Expect (s => s.GenerateMethodAndMoveBody (methodModule)).
          Return (new MethodDefinition ("GeneratedMethod", MethodAttributes.Public | MethodAttributes.Virtual, methodModule.ReturnType));

      _methodsVirtualizer.Transform (_tracker);

      _markingAttributeStrategy.AssertWasCalled (s => s.AddCustomAttribute (methodModule, _assemblyDefinition));
      _codeGenerator.AssertWasCalled (s => s.GenerateMethodAndMoveBody (methodModule));
    }

  }

}