// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace MarkVirtual
{

  public class Arguments
  {

    private StringDictionary Parameters;

    public Arguments (string[] Args)
    {
      Parameters = new StringDictionary ();
      Regex Spliter = new Regex (@"^-{1,2}|^/|=|:",
          RegexOptions.IgnoreCase | RegexOptions.Compiled);

      Regex Remover = new Regex (@"^['""]?(.*?)['""]?$",
          RegexOptions.IgnoreCase | RegexOptions.Compiled);

      string Parameter = null;
      string[] Parts;

      // Valid parameters forms:

      // {-,/,--}param{ ,=,:}((",')value(",'))

      // Examples: 

      // -param1 value1 --param2 /param3:"Test-:-work" 

      //   /param4=happy -param5 '--=nice=--'

      foreach (string Txt in Args)
      {
        Parts = Spliter.Split (Txt, 3);

        switch (Parts.Length)
        {
          case 1:
            if (Parameter != null)
            {
              if (!Parameters.ContainsKey (Parameter))
              {
                Parts[0] =
                    Remover.Replace (Parts[0], "$1");

                Parameters.Add (Parameter, Parts[0]);
              }
              Parameter = null;
            }
            break;
          case 2:
            if (Parameter != null)
            {
              if (!Parameters.ContainsKey (Parameter))
                Parameters.Add (Parameter, "true");
            }
            Parameter = Parts[1];
            break;
          case 3:
            if (Parameter != null)
            {
              if (!Parameters.ContainsKey (Parameter))
                Parameters.Add (Parameter, "true");
            }

            Parameter = Parts[1];
            if (!Parameters.ContainsKey (Parameter))
            {
              Parts[2] = Remover.Replace (Parts[2], "$1");
              Parameters.Add (Parameter, Parts[2]);
            }

            Parameter = null;
            break;
        }
      }
      if (Parameter != null)
      {
        if (!Parameters.ContainsKey (Parameter))
          Parameters.Add (Parameter, "true");
      }
    }
    public string this[string Param]
    {
      get
      {
        return (Parameters[Param]);
      }
    }
  }
}
