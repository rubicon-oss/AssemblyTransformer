// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using AssemblyTransformer;
using AssemblyTransformer.AppDomainBroker;
using AssemblyTransformer.AssemblyTransformations;
using AssemblyTransformer.FileSystem;
using NewTransformer.InfoProvider;
using NewTransformer.NewStatementReplacing;

namespace NewTransformer
{

  public class NewTransformerFactory : IAssemblyTransformationFactory
  {
    private readonly IFileSystem _fileSystem;
    private string _infoProviderName;
    private bool _factoryMakeProtected = false;

    public NewTransformerFactory (IFileSystem fileSystem, string workingDirectory)
    {
      ArgumentUtility.CheckNotNull ("fileSystem", fileSystem);
      _fileSystem = fileSystem;
    }

    public void AddOptions (OptionSet options)
    {
      ArgumentUtility.CheckNotNull ("options", options);

      options.Add (
            "newInfo=",
            "The assembly qualified name of the info provider class for the NewTransformer.",
            n => _infoProviderName = n);
      options.Add (
            "newWrap",
            "Make ctor protected and introduce factory method.",
            f => _factoryMakeProtected = (f != null));
    }


    public IAssemblyTransformation CreateTransformation (IAppDomainInfoBroker infoBroker)
    {
      if (_infoProviderName == null)
        throw new InvalidOperationException ("Please initialize options first! (NewTransformer) : Infoprovider must be specified");

      var provider = infoBroker.CreateInfoProviderWrapper<NewTransformerInfoProvider> (_infoProviderName, typeof (MemberID).Assembly.CodeBase);
      var wrapper = new NewTransformerInfoWrapper (provider);

      return new NewTransformer (wrapper, new ILCodeRewriter(), _factoryMakeProtected);
    }
  }
}