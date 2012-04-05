AssemblyTransformer
-------------------

This is the main solution. It contains the following C# projects:
- AssemblyTransformer - the actual AssemblyTransformer.exe program used to transform assemblies
- AssemblyTransformer.UnitTests
- MethodVirtualizer - A predefined transformation that allows non-virtual methods to be overridden by subclass proxies. To allow this, the implementation of a non-virtual method is moved to a new, virtual method with an unspeakable name. Tools can then generate overrides for the virtual method, but users of the class still cannot override it (because its name is unspeakable). This allows keeping the "non-virtual" contract of the method, while still allowing tools to override it. A type configured via the command line (info provider) can define what methods should be made virtual, and they can supply a custom attribute pointing from the unspeakable virtual method back to the "actual" non-virtual method (so that tools can recognize the connection between the methods).
- NewTransformer - A predefined transformation that adds static factory methods to certain transformed types and replaces "new" statements instantiating those types with calls to a factory. That factory could, e.g., create subclass proxies to stand in for the objects to be created. A type configured via the command line (info provider) can define for what constructors static factory methods should be created and how they should be called. It also defines the factory used to replace "new" statements.

TODO:

- One test is failing:

AssemblyTransformer.UnitTests.TransformationFactoryFactoryTest.DLLBasedTransformationFactoryFactoryTest.CreateTransformationFactories_WithRealAssembly' failed:
	System.TypeLoadException : Method 'CreateTransformation' in type 'AssemblyMethodsVirtualizer.AssemblyMethodVirtualizerFactory' from assembly 'AssemblyMethodsVirtualizer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' does not have an implementation.
	at AssemblyTransformer.UnitTests.TransformationFactoryFactoryTest.DLLBasedTransformationFactoryFactoryTest.CreateTransformationFactories_WithRealAssembly()

=> This is probably due to an error in "AssemblyTransformer\prereq\testing\transformation\AssemblyMethodsVirtualizer.dll".

AssemblyMethodsVirtualizer
--------------------------

This is a transformation that makes non-virtual methods virtual so that they can, e.g., be overridden by a subclass proxy. The transformation can be configured via the command line to use different "selection strategies" to determine what methods should be made virtual (based on declaring class, custom attributes, or a regular expression match). The command line can also choose a "marking strategy" to mark changed methods with a custom attribute (using an existing attribute or a newly generated attribute).

TODO: This seems to be an alternative version of AssemblyTransformer.MethodVirtualizer which uses predefined strategies rather than a user-specified class to select methods to be made virtual. Consider removing, unless the predefined strategies are valuable. Even then, consider unifying.

NOTE: This project has unit tests, whereas AssemblyTransformer.MethodVirtualizer has none. Before removing, the unit tests should probably be ported.

TODO: Several tests are failing:

Test 'AssemblyMethodsVirtualizer.UnitTests.AssemblyVirtualizingTest.CustomMarkingAttributeStrategyTest.OverrideMethods_AttributeTypeNotFound_Exception' failed:
	SetUp : System.IO.FileNotFoundException : Could not find file 'C:\Development\external\AssemblyTransformer\AssemblyMethodsVirtualizer\prereq\testing\DummyGenericAttribute.dll'.
	at System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
	at System.IO.FileStream.Init(String path, FileMode mode, FileAccess access, Int32 rights, Boolean useRights, FileShare share, Int32 bufferSize, FileOptions options, SECURITY_ATTRIBUTES secAttrs, String msgPath, Boolean bFromProxy, Boolean useLongPath)
	at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access, FileShare share)
	at Mono.Cecil.ModuleDefinition.GetFileStream(String fileName, FileMode mode, FileAccess access, FileShare share)
	at Mono.Cecil.ModuleDefinition.ReadModule(String fileName, ReaderParameters parameters)
	at Mono.Cecil.ModuleDefinition.ReadModule(String fileName)
	AssemblyVirtualizingTest\CustomMarkingAttributeStrategyTest.cs(23,0): at AssemblyMethodsVirtualizer.UnitTests.AssemblyVirtualizingTest.CustomMarkingAttributeStrategyTest.SetUp()

Test 'AssemblyMethodsVirtualizer.UnitTests.AssemblyVirtualizingTest.CustomMarkingAttributeStrategyTest.OverrideMethods_BothModules_MethodsMarked' failed:
	SetUp : System.IO.FileNotFoundException : Could not find file 'C:\Development\external\AssemblyTransformer\AssemblyMethodsVirtualizer\prereq\testing\DummyGenericAttribute.dll'.
	at System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
	at System.IO.FileStream.Init(String path, FileMode mode, FileAccess access, Int32 rights, Boolean useRights, FileShare share, Int32 bufferSize, FileOptions options, SECURITY_ATTRIBUTES secAttrs, String msgPath, Boolean bFromProxy, Boolean useLongPath)
	at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access, FileShare share)
	at Mono.Cecil.ModuleDefinition.GetFileStream(String fileName, FileMode mode, FileAccess access, FileShare share)
	at Mono.Cecil.ModuleDefinition.ReadModule(String fileName, ReaderParameters parameters)
	at Mono.Cecil.ModuleDefinition.ReadModule(String fileName)
	AssemblyVirtualizingTest\CustomMarkingAttributeStrategyTest.cs(23,0): at AssemblyMethodsVirtualizer.UnitTests.AssemblyVirtualizingTest.CustomMarkingAttributeStrategyTest.SetUp()

Test 'AssemblyMethodsVirtualizer.UnitTests.AssemblyVirtualizingTest.CustomMarkingAttributeStrategyTest.OverrideMethods_MainModule_MethodMarked' failed:
	SetUp : System.IO.FileNotFoundException : Could not find file 'C:\Development\external\AssemblyTransformer\AssemblyMethodsVirtualizer\prereq\testing\DummyGenericAttribute.dll'.
	at System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
	at System.IO.FileStream.Init(String path, FileMode mode, FileAccess access, Int32 rights, Boolean useRights, FileShare share, Int32 bufferSize, FileOptions options, SECURITY_ATTRIBUTES secAttrs, String msgPath, Boolean bFromProxy, Boolean useLongPath)
	at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access, FileShare share)
	at Mono.Cecil.ModuleDefinition.GetFileStream(String fileName, FileMode mode, FileAccess access, FileShare share)
	at Mono.Cecil.ModuleDefinition.ReadModule(String fileName, ReaderParameters parameters)
	at Mono.Cecil.ModuleDefinition.ReadModule(String fileName)
	AssemblyVirtualizingTest\CustomMarkingAttributeStrategyTest.cs(23,0): at AssemblyMethodsVirtualizer.UnitTests.AssemblyVirtualizingTest.CustomMarkingAttributeStrategyTest.SetUp()

Test 'AssemblyMethodsVirtualizer.UnitTests.AssemblyVirtualizingTest.CustomMarkingAttributeStrategyTest.OverrideMethods_SecondaryModule_MethodMarked' failed:
	SetUp : System.IO.FileNotFoundException : Could not find file 'C:\Development\external\AssemblyTransformer\AssemblyMethodsVirtualizer\prereq\testing\DummyGenericAttribute.dll'.
	at System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
	at System.IO.FileStream.Init(String path, FileMode mode, FileAccess access, Int32 rights, Boolean useRights, FileShare share, Int32 bufferSize, FileOptions options, SECURITY_ATTRIBUTES secAttrs, String msgPath, Boolean bFromProxy, Boolean useLongPath)
	at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access, FileShare share)
	at Mono.Cecil.ModuleDefinition.GetFileStream(String fileName, FileMode mode, FileAccess access, FileShare share)
	at Mono.Cecil.ModuleDefinition.ReadModule(String fileName, ReaderParameters parameters)
	at Mono.Cecil.ModuleDefinition.ReadModule(String fileName)
	AssemblyVirtualizingTest\CustomMarkingAttributeStrategyTest.cs(23,0): at AssemblyMethodsVirtualizer.UnitTests.AssemblyVirtualizingTest.CustomMarkingAttributeStrategyTest.SetUp()

=> This is due to a missing file: 'AssemblyMethodsVirtualizer\prereq\testing\DummyGenericAttribute.dll'.


ConstructorGenerator
--------------------

This is a transformation that adds static factory methods to certain transformed types and replaces "new" statements instantiating those types with calls to a factory. That factory could, e.g., create subclass proxies to stand in for the objects to be created. The transformation can be configured via the command line to choose the factory method to be used.

TODO: This seems to be an alternative version of AssemblyTransformer.NewTransformation. Consider removing, unless this version has valuable features. Even then, consider unifying.

NOTE: This project has unit tests, whereas AssemblyTransformer.NewTransformation has none. Before removing, the unit tests should probably be ported.

TODO: There are files (NewTransformerInfoWrapper\*.*) not included in the project.

TODO: The unit test project does not compile:

ConstructorGeneratorUnitTests\MethodReferenceGeneratorTest\MethodReferenceGeneratorTest.cs(89,23): error CS1501: No overload for method 'GetOrCreateParamList' takes 2 arguments
ConstructorGeneratorUnitTests\Utils\AssemblyDefinitionObjectMother.cs(62,24): error CS1729: 'Mono.Cecil.ExportedType' does not contain a constructor that takes 3 arguments
ConstructorGeneratorUnitTests\ConstructorGeneratorTest\ConstructorGeneratorFactoryTest.cs(30,18): error CS1729: 'ConstructorGenerator.ConstructorGeneratorFactory' does not contain a constructor that takes 1 arguments

TransformerTarget
-----------------

This is a sample solution used to test the AssemblyTransformer. To update the AssemblyTransformer binaries used by this project, replace the files in the prereq\transformer folder.

NOTE: There is a circular dependency between ExampleLib and TransformerTargetInfo. Therefore, building the solution for the first time will fail. It must be built two times to succeeed.

TODO: There is a project missing from the solution: CecilvsReflection.