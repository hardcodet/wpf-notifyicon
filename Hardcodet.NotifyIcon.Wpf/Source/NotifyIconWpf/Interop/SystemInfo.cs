using System;
using System.Windows.Interop;

namespace Hardcodet.Wpf.TaskbarNotification.Interop
{
  public static class SystemInfo
  {
    private static Tuple<double, double> dpiFactors;

    private static Tuple<double, double> DpiFactors
    {
      get
      {
        if (dpiFactors == null)        
          using (var source = new HwndSource(new HwndSourceParameters()))
            dpiFactors = Tuple.Create(source.CompositionTarget.TransformToDevice.M11, source.CompositionTarget.TransformToDevice.M22);        
        return dpiFactors;
      }
    }

    public static double DpiXFactor
    {
      get
      {
        var factors = DpiFactors;
        return factors != null ? factors.Item1 : 1;
      }
    }

    public static double DpiYFactor
    {
      get
      {
        var factors = DpiFactors;
        return factors != null ? factors.Item2 : 1;
      }
    }         
  }
}
