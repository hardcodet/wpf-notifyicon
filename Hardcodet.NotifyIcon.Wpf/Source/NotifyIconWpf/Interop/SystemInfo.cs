using System.Windows.Interop;

namespace Hardcodet.Wpf.TaskbarNotification.Interop
{
  public static class SystemInfo
  {
    private static System.Windows.Point? dpiFactors;

    private static System.Windows.Point? DpiFactors
    {
      get
      {
        if (dpiFactors == null)        
          using (var source = new HwndSource(new HwndSourceParameters()))
            dpiFactors = new System.Windows.Point(source.CompositionTarget.TransformToDevice.M11, source.CompositionTarget.TransformToDevice.M22);        
        return dpiFactors;
      }
    }

    public static double DpiXFactor
    {
      get
      {
        var factors = DpiFactors;
        return factors.HasValue ? factors.Value.X : 1;
      }
    }

    public static double DpiYFactor
    {
      get
      {
        var factors = DpiFactors;
        return factors.HasValue ? factors.Value.Y : 1;
      }
    }         
  }
}
