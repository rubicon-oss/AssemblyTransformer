// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using AssemblyMethodsVirtualizer.TargetSelection.SelectionStrategies;
using AssemblyTransformer;
using AssemblyTransformer.AssemblyTracking;

namespace AssemblyMethodsVirtualizer.TargetSelection
{
  public class TargetSelectorFactory : ITargetSelectionFactory
  {
    private string _regex = null;
    private string _attributeName = null;
    private readonly List<string> _classNames = new List<string> ();

    public TargetSelectorFactory ()
    {
    }

    public void AddOptions (OptionSet options)
    {
      options.Add (
       "regex=",
       "The regular expression matching the targeted methods full name.",
       r => _regex = r);
      options.Add (
       "selectionAttribute=",
       "The fullname of the attribute marking the classes (their subclasses are included) or methods that should be made virtual.",
       atName => _attributeName = atName);
      options.Add (
       "scn|selectionClassName=",
       "The fullname(s) of the classes (their subclasses are included) that should be made virtual.",
       name => _classNames.Add (name));
    }

    public ITargetSelectionStrategy CreateSelector (IAssemblyTracker tracker)
    {
      ArgumentUtility.CheckNotNull ("tracker", tracker);

      var strategies = new List<ITargetSelectionStrategy> ();
      var visitorStrategies = new List<IVisitorTargetSelectionStrategy> ();

      if (_regex != null)
        strategies.Add (new RegularExpressionSelectionStrategy (_regex));

      if (_classNames.Count > 0)
        visitorStrategies.Add (new ClassNameSelectionStrategy (_classNames));
      if (_attributeName != null)
        visitorStrategies.Add (new AttributeNameSelectionStrategy (_attributeName));

      if (visitorStrategies.Count > 0)
        strategies.Add (new ClassHierarchyVisitorStrategy(tracker, visitorStrategies));

      return new TargetSelector(strategies);
    }
  }
}