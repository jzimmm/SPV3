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
using HXE.SPV3;

namespace SPV3
{
  public partial class Configuration
  {
    public ConfigurationLoader    Loader    { get; set; } = new ConfigurationLoader();
    public ConfigurationKernel    Kernel    { get; set; } = new ConfigurationKernel();
    public ConfigurationShaders   Shaders   { get; set; } = new ConfigurationShaders();
    public ConfigurationOpenSauce OpenSauce { get; set; } = new ConfigurationOpenSauce();

    public void Load()
    {
      Loader.Load();
      OpenSauce.Load();

      if (File.Exists(HXE.Paths.Configuration))
      {
        var kernel = (HXE.Configuration) HXE.Paths.Configuration;
        kernel.Load();

        /* core */
        {
          Kernel.SkipVerifyMainAssets = kernel.Kernel.SkipVerifyMainAssets;
          Kernel.SkipInvokeCoreTweaks = kernel.Kernel.SkipInvokeCoreTweaks;
          Kernel.SkipResumeCheckpoint = kernel.Kernel.SkipResumeCheckpoint;
          Kernel.SkipSetShadersConfig = kernel.Kernel.SkipSetShadersConfig;
          Kernel.SkipInvokeExecutable = kernel.Kernel.SkipInvokeExecutable;
          Kernel.SkipPatchLargeAAware = kernel.Kernel.SkipPatchLargeAAware;
          Kernel.EnableSpv3KernelMode = kernel.Kernel.EnableSpv3KernelMode;
          Kernel.EnableSpv3LegacyMode = kernel.Kernel.EnableSpv3LegacyMode;
        }

        /* shaders */
        {
          Shaders.DynamicLensFlares = kernel.PostProcessing.DynamicLensFlares;
          Shaders.Volumetrics       = kernel.PostProcessing.Volumetrics;
          Shaders.LensDirt          = kernel.PostProcessing.LensDirt;
          Shaders.HudVisor          = kernel.PostProcessing.HudVisor;
          Shaders.FilmGrain         = kernel.PostProcessing.FilmGrain;

          switch (kernel.PostProcessing.Mxao)
          {
            case PostProcessing.MxaoOptions.Off:
              Shaders.Mxao = 0;
              break;
            case PostProcessing.MxaoOptions.Low:
              Shaders.Mxao = 1;
              break;
            case PostProcessing.MxaoOptions.High:
              Shaders.Mxao = 2;
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }

          switch (kernel.PostProcessing.MotionBlur)
          {
            case PostProcessing.MotionBlurOptions.Off:
              Shaders.MotionBlur = 0;
              break;
            case PostProcessing.MotionBlurOptions.BuiltIn:
              Shaders.MotionBlur = 1;
              break;
            case PostProcessing.MotionBlurOptions.PombLow:
              Shaders.MotionBlur = 2;
              break;
            case PostProcessing.MotionBlurOptions.PombHigh:
              Shaders.MotionBlur = 3;
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }

          switch (kernel.PostProcessing.Dof)
          {
            case PostProcessing.DofOptions.Off:
              Shaders.Dof = 0;
              break;
            case PostProcessing.DofOptions.Low:
              Shaders.Dof = 1;
              break;
            case PostProcessing.DofOptions.High:
              Shaders.Dof = 2;
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
        }
      }
    }

    public void Save()
    {
      var kernel = (HXE.Configuration) HXE.Paths.Configuration;

      /* core */
      {
        kernel.Kernel.SkipVerifyMainAssets = Kernel.SkipVerifyMainAssets;
        kernel.Kernel.SkipInvokeCoreTweaks = Kernel.SkipInvokeCoreTweaks;
        kernel.Kernel.SkipResumeCheckpoint = Kernel.SkipResumeCheckpoint;
        kernel.Kernel.SkipSetShadersConfig = Kernel.SkipSetShadersConfig;
        kernel.Kernel.SkipInvokeExecutable = Kernel.SkipInvokeExecutable;
        kernel.Kernel.SkipPatchLargeAAware = Kernel.SkipPatchLargeAAware;
        kernel.Kernel.EnableSpv3KernelMode = Kernel.EnableSpv3KernelMode;
        kernel.Kernel.EnableSpv3LegacyMode = Kernel.EnableSpv3LegacyMode;
      }

      /* shaders */
      {
        kernel.PostProcessing.DynamicLensFlares = Shaders.DynamicLensFlares;
        kernel.PostProcessing.Volumetrics       = Shaders.Volumetrics;
        kernel.PostProcessing.LensDirt          = Shaders.LensDirt;
        kernel.PostProcessing.HudVisor          = Shaders.HudVisor;
        kernel.PostProcessing.FilmGrain         = Shaders.FilmGrain;

        switch (Shaders.Mxao)
        {
          case 0:
            kernel.PostProcessing.Mxao = PostProcessing.MxaoOptions.Off;
            break;
          case 1:
            kernel.PostProcessing.Mxao = PostProcessing.MxaoOptions.Low;
            break;
          case 2:
            kernel.PostProcessing.Mxao = PostProcessing.MxaoOptions.High;
            break;
        }

        switch (Shaders.MotionBlur)
        {
          case 0:
            kernel.PostProcessing.MotionBlur = PostProcessing.MotionBlurOptions.Off;
            break;
          case 1:
            kernel.PostProcessing.MotionBlur = PostProcessing.MotionBlurOptions.BuiltIn;
            break;
          case 2:
            kernel.PostProcessing.MotionBlur = PostProcessing.MotionBlurOptions.PombLow;
            break;
          case 3:
            kernel.PostProcessing.MotionBlur = PostProcessing.MotionBlurOptions.PombHigh;
            break;
        }

        switch (Shaders.Dof)
        {
          case 0:
            kernel.PostProcessing.Dof = PostProcessing.DofOptions.Off;
            break;
          case 1:
            kernel.PostProcessing.Dof = PostProcessing.DofOptions.Low;
            break;
          case 2:
            kernel.PostProcessing.Dof = PostProcessing.DofOptions.High;
            break;
        }
      }

      kernel.Save();
      Loader.Save();
      OpenSauce.Save();
    }
  }
}