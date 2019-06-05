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
    private readonly Home _home;
    
    public HomeUserControl()
    {
      InitializeComponent();
      _home = (Home) DataContext;
    }

    private void ViewVersion(object sender, MouseButtonEventArgs e)
    {
      Process.Start(_home.VersionAddress);
    }
  }
}