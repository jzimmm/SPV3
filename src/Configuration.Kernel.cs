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

namespace SPV3
{
  public partial class Configuration
  {
    public partial class ConfigurationKernel
    {
      public ConfigurationKernelCore    Core    { get; set; } = new ConfigurationKernelCore();
      public ConfigurationKernelShaders Shaders { get; set; } = new ConfigurationKernelShaders();

      public void Save()
      {
        var configuration = (HXE.Configuration) HXE.Paths.Configuration;

        /* core */
        {
          configuration.Kernel.SkipVerifyMainAssets = Core.SkipVerifyMainAssets;
          configuration.Kernel.SkipInvokeCoreTweaks = Core.SkipInvokeCoreTweaks;
          configuration.Kernel.SkipResumeCheckpoint = Core.SkipResumeCheckpoint;
          configuration.Kernel.SkipSetShadersConfig = Core.SkipSetShadersConfig;
          configuration.Kernel.SkipInvokeExecutable = Core.SkipInvokeExecutable;
          configuration.Kernel.SkipPatchLargeAAware = Core.SkipPatchLargeAAware;
          configuration.Kernel.EnableSpv3KernelMode = Core.EnableSpv3KernelMode;
          configuration.Kernel.EnableSpv3LegacyMode = Core.EnableSpv3LegacyMode;
        }

        /* shaders */
        {
          configuration.PostProcessing.DynamicLensFlares = Shaders.DynamicLensFlares;
          configuration.PostProcessing.Volumetrics       = Shaders.Volumetrics;
          configuration.PostProcessing.LensDirt          = Shaders.LensDirt;
          configuration.PostProcessing.HudVisor          = Shaders.HudVisor;
          configuration.PostProcessing.FilmGrain         = Shaders.FilmGrain;
        }

        configuration.Save();
      }

      public void Load()
      {
        var configuration = (HXE.Configuration) HXE.Paths.Configuration;
        configuration.Load();

        /* core */
        {
          Core.SkipVerifyMainAssets = configuration.Kernel.SkipVerifyMainAssets;
          Core.SkipInvokeCoreTweaks = configuration.Kernel.SkipInvokeCoreTweaks;
          Core.SkipResumeCheckpoint = configuration.Kernel.SkipResumeCheckpoint;
          Core.SkipSetShadersConfig = configuration.Kernel.SkipSetShadersConfig;
          Core.SkipInvokeExecutable = configuration.Kernel.SkipInvokeExecutable;
          Core.SkipPatchLargeAAware = configuration.Kernel.SkipPatchLargeAAware;
          Core.EnableSpv3KernelMode = configuration.Kernel.EnableSpv3KernelMode;
          Core.EnableSpv3LegacyMode = configuration.Kernel.EnableSpv3LegacyMode;
        }

        /* shaders */
        {
          Shaders.DynamicLensFlares = configuration.PostProcessing.DynamicLensFlares;
          Shaders.Volumetrics       = configuration.PostProcessing.Volumetrics;
          Shaders.LensDirt          = configuration.PostProcessing.LensDirt;
          Shaders.HudVisor          = configuration.PostProcessing.HudVisor;
          Shaders.FilmGrain         = configuration.PostProcessing.FilmGrain;
        }
      }
    }
  }
}