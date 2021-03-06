// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
#define PERFORMANCE_OUTPUT

using System;
using System.Diagnostics;
using AssemblyTransformer;
using AssemblyTransformer.AssemblyTransformations;
using AssemblyTransformer.FileSystem;
using ConstructorGenerator.CodeGenerator;
using ConstructorGenerator.MixinChecker;
using ConstructorGenerator.ReferenceGenerator;

namespace ConstructorGenerator
{
  public class ConstructorGeneratorFactory : IAssemblyTransformationFactory
  {
    private readonly IFileSystem _fileSystem;
    private string _objectFactory = "Remotion.Mixins.ObjectFactory";
    private string _paramList = "Remotion.Reflection.ParamList";
    private string _remotionInterfaces = null;
    private readonly string _workingDir = null;

    public ConstructorGeneratorFactory (IFileSystem fileSystem, string workingDirectory)
    {
      ArgumentUtility.CheckNotNull ("fileSystem", fileSystem);
      _fileSystem = fileSystem;
      _workingDir = workingDirectory;
    }

    public void AddOptions (OptionSet options)
    {
      ArgumentUtility.CheckNotNull ("options", options);

      options.Add (
            "fac|objectFactory=",
            "The FullName of the objectFactory providing the 'Create' method. default: 'Remotion.Mixins.ObjectFactory'",
            fac => _objectFactory = fac);
      options.Add (
            "par|paramList=",
            "The FullName of the paramList providing the 'Create' method needed for the objectFactory argument. default: 'Remotion.Reflection.ParamList'",
            par => _paramList = par);
      options.Add (
            "rem|remotionInterfaces=",
            "The FullName of the Assembly in which the MixinConfiguration and ObjectFactory/ParamList can be found. "+
            "This is only needed if there is no reference to remotion.interfaces in the target modules.",
            rem => _remotionInterfaces = rem);
    }

    public IAssemblyTransformation CreateTransformation ()
    {
      if (_remotionInterfaces == null)
        throw new InvalidOperationException ("Please initialize options first! (ConstructorGenerator) : Remotion interfaces must be specified");
      if (_workingDir == null)
        throw new InvalidOperationException ("Please initialize options first! (ConstructorGenerator) : Working directory must be specified");

#if PERFORMANCE_OUTPUT
      Stopwatch s = new Stopwatch ();
      s.Start ();
#endif
      try
      {
        var checker = new StringAndReflectionBasedMixinChecker (_workingDir, _remotionInterfaces);

        return new ConstructorGenerator (
            checker,
            new ILCodeGenerator (
                new MethodReferenceGenerator (_remotionInterfaces, _objectFactory, _paramList),
                checker)
          );
      }
      finally 
      {
#if PERFORMANCE_OUTPUT
        s.Stop ();
        Console.WriteLine ("time for mixinconfiguration initialization : " + s.Elapsed);
        s.Restart ();
#endif
      }

    }
  }
}