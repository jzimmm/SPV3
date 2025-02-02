﻿/**
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
using System.IO;
using System.Linq;
using System.Windows;

namespace SPV3
{
  /// <summary>
  ///   Interaction logic for App.xaml
  /// </summary>
  public partial class App
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      try
      {
        Environment.CurrentDirectory = File.Exists(Paths.Installation)
          ? File.ReadAllText(Paths.Installation)
          : Environment.CurrentDirectory;
      }
      catch (Exception)
      {
        Environment.CurrentDirectory = Environment.CurrentDirectory;
      }

      if (e.Args.Any(arg => arg.Equals("-auto")))
      {
        new Main().Invoke();
        Environment.Exit(0);
      }

      base.OnStartup(e);
    }
  }
}