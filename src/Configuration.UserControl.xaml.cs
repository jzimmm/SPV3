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
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SPV3
{
  public partial class Configuration_UserControl : UserControl
  {
    private readonly Configuration _configuration;

    public Configuration_UserControl()
    {
      InitializeComponent();
      _configuration = (Configuration) DataContext;
      _configuration.Load();
    }

    public event EventHandler Home;

    private void Save(object sender, RoutedEventArgs e)
    {
      _configuration.Save();
      Home?.Invoke(sender, e);
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
      Home?.Invoke(sender, e);
    }

    private void CalculateFOV(object sender, RoutedEventArgs e)
    {
      _configuration.CalculateFOV();
    }

    private void ResetWeaponPositions(object sender, RoutedEventArgs e)
    {
      _configuration.ResetWeaponPositions();
    }

    private void InstallOpenSauce(object sender, RoutedEventArgs e)
    {
      try
      {
        new AmaiSosu
        {
          Path = Path.Combine(Environment.CurrentDirectory, Paths.AmaiSosu)
        }.Execute();
      }
      catch (Exception exception)
      {
        MessageBox.Show(exception.Message);
      }
    }

    private void GBufferUnchecked(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("WARNING: VISR and Thermal Vision will not work. " +
                      "This should only be used on low-end computers as a last resort.");
    }

    private void PresetVeryLow(object sender, RoutedEventArgs e)
    {
      _configuration.OpenSauce.GBuffer          = false;
      _configuration.Shaders.FilmGrain          = false;
      _configuration.Shaders.VolumetricLighting = false;
      _configuration.Shaders.LensDirt           = false;
      _configuration.Shaders.DynamicLensFlares  = false;
      _configuration.Shaders.MotionBlur         = 0;
      _configuration.Shaders.DOF                = 0;
      _configuration.Shaders.MXAO               = 0;
    }

    private void PresetLow(object sender, RoutedEventArgs e)
    {
      _configuration.OpenSauce.GBuffer          = true;
      _configuration.Shaders.FilmGrain          = false;
      _configuration.Shaders.VolumetricLighting = true;
      _configuration.Shaders.LensDirt           = true;
      _configuration.Shaders.DynamicLensFlares  = false;
      _configuration.Shaders.MotionBlur         = 0;
      _configuration.Shaders.DOF                = 0;
      _configuration.Shaders.MXAO               = 0;
    }

    private void PresetMedium(object sender, RoutedEventArgs e)
    {
      _configuration.OpenSauce.GBuffer          = true;
      _configuration.Shaders.FilmGrain          = false;
      _configuration.Shaders.VolumetricLighting = true;
      _configuration.Shaders.LensDirt           = true;
      _configuration.Shaders.DynamicLensFlares  = false;
      _configuration.Shaders.MotionBlur         = 1;
      _configuration.Shaders.DOF                = 1;
      _configuration.Shaders.MXAO               = 0;
    }

    private void PresetHigh(object sender, RoutedEventArgs e)
    {
      _configuration.OpenSauce.GBuffer          = true;
      _configuration.Shaders.FilmGrain          = true;
      _configuration.Shaders.VolumetricLighting = true;
      _configuration.Shaders.LensDirt           = true;
      _configuration.Shaders.DynamicLensFlares  = false;
      _configuration.Shaders.MotionBlur         = 2;
      _configuration.Shaders.DOF                = 1;
      _configuration.Shaders.MXAO               = 1;
    }

    private void PresetVeryHigh(object sender, RoutedEventArgs e)
    {
      _configuration.OpenSauce.GBuffer          = true;
      _configuration.Shaders.FilmGrain          = true;
      _configuration.Shaders.VolumetricLighting = true;
      _configuration.Shaders.LensDirt           = true;
      _configuration.Shaders.DynamicLensFlares  = false;
      _configuration.Shaders.MotionBlur         = 3;
      _configuration.Shaders.DOF                = 2;
      _configuration.Shaders.MXAO               = 2;
    }

    private void PresetUltra(object sender, RoutedEventArgs e)
    {
      _configuration.OpenSauce.GBuffer          = true;
      _configuration.Shaders.FilmGrain          = true;
      _configuration.Shaders.VolumetricLighting = true;
      _configuration.Shaders.LensDirt           = true;
      _configuration.Shaders.DynamicLensFlares  = true;
      _configuration.Shaders.MotionBlur         = 3;
      _configuration.Shaders.DOF                = 2;
      _configuration.Shaders.MXAO               = 2;
    }
  }
}