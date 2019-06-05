using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Reflection.Assembly;

namespace SPV3
{
  /// <summary>
  ///   Interaction logic for HomeUserControl.xaml
  /// </summary>
  public partial class HomeUserControl : UserControl
  {
    public HomeUserControl()
    {
      InitializeComponent();
    }

    private void ViewVersion(object sender, MouseButtonEventArgs e)
    {
      Process.Start($"https://github.com/yumiris/SPV3/tree/build-{GetEntryAssembly()?.GetName().Version.Major:D4}");
    }
  }
}