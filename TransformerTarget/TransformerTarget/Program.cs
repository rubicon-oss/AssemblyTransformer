using System;
using ExampleLib;
using MyMixin;
using TransformerTargetInfo;

namespace TransformerTarget
{
  class Program
  {
    static void Main (string[] args)
    {
      var x = typeof (NewInfoProvider);
      Console.WriteLine ("------ instantiating MixinTargetClass");

      var target = new MixinTargetClass();

      Console.WriteLine (" calling CalculateSomething from class");
      Console.WriteLine (target.CalculateSomething());

      Console.WriteLine (" calling CalculateAnother from Mixin");
      Console.WriteLine (((IMyMixin)target).CalculateAnother ());

      Console.WriteLine ("-----------------");
    }

    public void PerformSmth (string x)
    {
      var y = String.Concat ("PerformSmth ", x);
      Console.WriteLine (y);
      double result = 5 * 4;
      Console.WriteLine (y + " result = " + result);
    }
  }
}
