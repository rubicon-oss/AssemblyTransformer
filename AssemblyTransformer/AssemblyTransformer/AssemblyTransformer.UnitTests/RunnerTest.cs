// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq;
using AssemblyTransformer.AssemblySigning;
using AssemblyTransformer.AssemblyTracking;
using AssemblyTransformer.AssemblyTransformations;
using NUnit.Framework;
using Rhino.Mocks;

namespace AssemblyTransformer.UnitTests
{
  [TestFixture]
  public class RunnerTest
  {
    private Runner _runner = new Runner();

    private IAssemblyTracker _trackerMock;
    private IAssemblyTrackerFactory _trackerFactoryMock;
    private IAssemblyTransformation _transformerMock;
    private IAssemblyTransformation _transformerMock2;
    private IAssemblyTransformationFactory _transformationFactoryMock;
    private IAssemblyTransformationFactory _transformationFactoryMock2;
    private IAssemblySigner _signerMock;
    private IAssemblySignerFactory _signerFactoryMock;

    [SetUp]
    public void SetUp ()
    {
      _trackerMock = MockRepository.GenerateStrictMock<IAssemblyTracker>();
      _trackerFactoryMock = MockRepository.GenerateStrictMock<IAssemblyTrackerFactory>();
      _transformerMock = MockRepository.GenerateStrictMock<IAssemblyTransformation>();
      _transformationFactoryMock = MockRepository.GenerateStrictMock<IAssemblyTransformationFactory>();
      _transformerMock2 = MockRepository.GenerateStrictMock<IAssemblyTransformation> ();
      _transformationFactoryMock2 = MockRepository.GenerateStrictMock<IAssemblyTransformationFactory> ();
      _signerMock = MockRepository.GenerateStrictMock<IAssemblySigner>();
      _signerFactoryMock = MockRepository.GenerateStrictMock<IAssemblySignerFactory>();
    }

    [Test]
    [ExpectedException (typeof(ArgumentNullException))]
    public void Runner_NullParameters ()
    {
      _runner.Run (null, null, null);
    }

    [Test]
    public void Runner_InstantiatesAndRuns ()
    {
      _trackerFactoryMock.Expect (mock => mock.CreateTracker()).Return (_trackerMock);
      _transformationFactoryMock.Expect (mock => mock.CreateTransformation()).Return (_transformerMock);
      _transformerMock.Expect (mock => mock.Transform (_trackerMock));
      _signerFactoryMock.Expect (mock => mock.CreateSigner()).Return (_signerMock);
      _signerMock.Expect (mock => mock.SignAndSave (_trackerMock));

      _trackerFactoryMock.Replay();
      _trackerMock.Replay();
      _transformationFactoryMock.Replay();
      _transformerMock.Replay();
      _signerFactoryMock.Replay();
      _signerMock.Replay();
      _runner.Run (_trackerFactoryMock, new [] { _transformationFactoryMock }, _signerFactoryMock);

      _trackerFactoryMock.VerifyAllExpectations();
      _trackerMock.VerifyAllExpectations();

      _transformationFactoryMock.VerifyAllExpectations();
      _transformerMock.VerifyAllExpectations();

      _signerFactoryMock.VerifyAllExpectations();
      _signerMock.VerifyAllExpectations();
    }

    [Test]
    public void Runner_InstantiatesAndRuns_MultipleTransformations ()
    {
      _trackerFactoryMock.Expect (mock => mock.CreateTracker ()).Return (_trackerMock);
      _transformationFactoryMock.Expect (mock => mock.CreateTransformation ()).Return (_transformerMock);
      _transformerMock.Expect (mock => mock.Transform (_trackerMock));
      _transformationFactoryMock2.Expect (mock => mock.CreateTransformation ()).Return (_transformerMock2);
      _transformerMock2.Expect (mock => mock.Transform (_trackerMock));
      _signerFactoryMock.Expect (mock => mock.CreateSigner ()).Return (_signerMock);
      _signerMock.Expect (mock => mock.SignAndSave (_trackerMock));

      _trackerFactoryMock.Replay ();
      _trackerMock.Replay ();
      _transformationFactoryMock.Replay ();
      _transformerMock.Replay ();
      _transformationFactoryMock2.Replay ();
      _transformerMock2.Replay ();
      _signerFactoryMock.Replay ();
      _signerMock.Replay ();
      _runner.Run (_trackerFactoryMock, new[] { _transformationFactoryMock, _transformationFactoryMock2 }, _signerFactoryMock);

      _trackerFactoryMock.VerifyAllExpectations ();
      _trackerMock.VerifyAllExpectations ();

      _transformationFactoryMock.VerifyAllExpectations ();
      _transformerMock.VerifyAllExpectations ();
      _transformationFactoryMock2.VerifyAllExpectations ();
      _transformerMock2.VerifyAllExpectations ();

      _signerFactoryMock.VerifyAllExpectations ();
      _signerMock.VerifyAllExpectations ();
    }

    [Test]
    public void Runner_InstantiatesAndRuns_NoTransformation ()
    {
      _trackerFactoryMock.Expect (mock => mock.CreateTracker ()).Return (_trackerMock);
      _signerFactoryMock.Expect (mock => mock.CreateSigner ()).Return (_signerMock);
      _signerMock.Expect (mock => mock.SignAndSave (_trackerMock));

      _trackerFactoryMock.Replay ();
      _trackerMock.Replay ();
      _signerFactoryMock.Replay ();
      _signerMock.Replay ();
      _runner.Run (_trackerFactoryMock, Enumerable.Empty<IAssemblyTransformationFactory>(), _signerFactoryMock);

      _trackerFactoryMock.VerifyAllExpectations ();
      _trackerMock.VerifyAllExpectations ();

      _signerFactoryMock.VerifyAllExpectations ();
      _signerMock.VerifyAllExpectations ();
    }
  }
}