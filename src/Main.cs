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
using System.Windows;
using static System.IO.File;
using static System.IO.Path;

namespace SPV3
{
  public partial class Main
  {
    public MainVersion Version { get; set; } = new MainVersion(); /* gets spv3 loader version     */
    public MainUpdate  Update  { get; set; } = new MainUpdate();  /* gets spv3 loader updates     */
    public MainError   Error   { get; set; } = new MainError();   /* catches & shows exceptions   */
    public MainInstall Install { get; set; } = new MainInstall(); /* checks & allows installation */
    public MainLoad    Load    { get; set; } = new MainLoad();    /* checks & allows loading      */

    /// <summary>
    ///   Wrapper for subclass initialisation methods.
    /// </summary>
    public void Initialise()
    {
      Version.Initialise();

      if (!Exists(HXE.Paths.HCE.Executable))
      {
        Load.Visibility = Visibility.Collapsed;

        if (Exists(Combine("data", HXE.Paths.Manifest)))
        {
          Install.Visibility = Visibility.Visible;
        }
        else
        {
          Error.Content    = "Please ensure this loader is in the appropriate SPV3 folder.";
          Error.Visibility = Visibility.Visible;
        }

        return;
      }

      Load.Visibility    = Visibility.Visible;
      Install.Visibility = Visibility.Collapsed;

      try
      {
        Update.Initialise();
      }
      catch (Exception e)
      {
        Error.Visibility = Visibility.Visible;
        Error.Content    = "Update error: " + e.Message.ToLower();
      }
    }

    /// <summary>
    ///   Wrapper for the load routine with UI support.
    /// </summary>
    public void Invoke()
    {
      try
      {
        Load.Invoke();
      }
      catch (Exception e)
      {
        WriteAllText(Paths.Exception, e.ToString());

        Error.Content    = $"Load error: {e.Message.ToLower()}\n\nClick here for more information.";
        Error.Visibility = Visibility.Visible;
      }
    }

    /// <summary>
    ///   Successfully exits the SPV3 loader.
    /// </summary>
    public void Quit()
    {
      Environment.Exit(0);
    }
  }
}