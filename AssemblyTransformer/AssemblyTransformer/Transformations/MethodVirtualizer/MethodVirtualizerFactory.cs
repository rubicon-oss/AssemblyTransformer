// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer;
using AssemblyTransformer.AppDomainBroker;
using AssemblyTransformer.AssemblyTransformations;
using AssemblyTransformer.FileSystem;
using MethodVirtualizer.ILCodeGeneration;
using MethodVirtualizer.InfoProvider;

namespace MethodVirtualizer
{
  public class MethodVirtualizerFactory : IAssemblyTransformationFactory
  {
    private string _infoProviderName = null;
    private IFileSystem _fileSystem;

    public MethodVirtualizerFactory (IFileSystem fileSystem, string workingDirectory)
    {
      ArgumentUtility.CheckNotNull ("fileSystem", fileSystem);
      _fileSystem = fileSystem;
    }

    public void AddOptions (OptionSet options)
    {
      ArgumentUtility.CheckNotNull ("options", options);

      options.Add (
            "virtualizerInfo=",
            "The assembly qualified name of the info provider class for the MethodVirtualizer.",
            n => _infoProviderName = n);
    }

    public IAssemblyTransformation CreateTransformation (IAppDomainInfoBroker infoBroker)
    {
      if (_infoProviderName == null)
        throw new InvalidOperationException ("Please initialize options first! (MethodVirtualizer) : Infoprovider must be specified");

      var provider = infoBroker.CreateInfoProviderWrapper<MethodVirtualizerInfoProvider> (_infoProviderName, typeof (MemberID).Assembly.CodeBase);
      var wrapper = new MethodVirtualizerInfoWrapper (provider);

      return new MethodVirtualizer (wrapper, new ILCodeGenerator());
    }
  }
}