/**
 * Copyright (c) 2019 Emilian Roman
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using HXE;
using MessageBox = System.Windows.MessageBox;

namespace SPV3
{
  public partial class InstallerWindow : Window
  {
    public InstallerWindow()
    {
      InitializeComponent();
      Target.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games",
        "Halo SPV3");
    }

    private void BrowseTarget(object sender, RoutedEventArgs e)
    {
      using (var dialog = new FolderBrowserDialog())
      {
        dialog.ShowDialog();
        Target.Text = dialog.SelectedPath;
      }
    }

    private void Install(object sender, RoutedEventArgs e)
    {
      Status.Content = "Installing SPV3 ...";
      
      switch (Cli.Start($"/install \"{Target.Text}\""))
      {
        case Exit.Code.Success:
          MessageBox.Show("SPV3 installation complete!");
          Process.Start(Path.Combine(Target.Text, "spv3.exe"));
          Environment.Exit(0);
          break;
        case Exit.Code.Exception:
          Status.Content = "Exception has occurred. Review log file.";
          break;
        case Exit.Code.InvalidInstall:
          Status.Content = "Could not detect a legal HCE installation.";
          break;
      }
    }
  }
}