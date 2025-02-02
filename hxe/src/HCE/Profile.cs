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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.IO.SearchOption;
using static HXE.Console;
using static HXE.HCE.Profile.ProfileAudio;
using static HXE.HCE.Profile.ProfileDetails;
using static HXE.HCE.Profile.ProfileNetwork;
using static HXE.HCE.Profile.ProfileVideo;
using static HXE.HCE.Profile.ProfileInput;
using static HXE.Paths;
using Directory = System.IO.Directory;

namespace HXE.HCE
{
  /// <inheritdoc />
  /// <summary>
  ///   Object representing a HCE profile blam.sav binary.
  /// </summary>
  public class Profile : File
  {
    public ProfileDetails Details { get; set; } = new ProfileDetails(); /* profile name & online player colour */
    public ProfileMouse   Mouse   { get; set; } = new ProfileMouse();   /* sensitivities & vertical axis inversion */
    public ProfileAudio   Audio   { get; set; } = new ProfileAudio();   /* volumes, qualities, varieties & eax/hw */
    public ProfileVideo   Video   { get; set; } = new ProfileVideo();   /* resolutions, rates, effects & qualities */
    public ProfileNetwork Network { get; set; } = new ProfileNetwork(); /* connection types & server/client ports */
    public ProfileInput   Input   { get; set; } = new ProfileInput();   /* input-action mapping */

    /// <summary>
    ///   Saves object state to the inbound file.
    /// </summary>
    public void Save()
    {
      /**
       * We first open up a binary writer for storing the configuration in the blam.sav binary. Data is all written in
       * one go to the binary on the filesystem.
       */

      using (var fs = new FileStream(Path, FileMode.Open, FileAccess.ReadWrite))
      using (var ms = new MemoryStream(8192))
      using (var bw = new BinaryWriter(ms))
      {
        void WriteBoolean(Offset offset, bool data)
        {
          ms.Position = (int) offset;
          bw.Write(data);
        }

        void WriteInteger(Offset offset, int data)
        {
          ms.Position = (int) offset;
          bw.Write(data);
        }

        void WriteByte(Offset offset, byte data)
        {
          ms.Position = (int) offset;
          bw.Write(data);
        }

        fs.Position = 0;
        fs.CopyTo(ms);

        /**
         * The name is stored in UTF-16; hence, we rely on the Unicode class to encode the string to a byte array for
         * writing the profile name in the binary.
         */

        ms.Position = (int) Offset.ProfileName;
        bw.Write(Encoding.Unicode.GetBytes(Details.Name));

        /**
         * First, we'll take care of the enum options. Storing them is rather straightforward: we cast their values to
         * integers, which can be then written to the binary. 
         */

        WriteInteger(Offset.ProfileColour,         (int) Details.Colour);
        WriteInteger(Offset.VideoFrameRate,        (int) Video.FrameRate);
        WriteInteger(Offset.VideoQualityParticles, (int) Video.Particles);
        WriteInteger(Offset.VideoQualityTextures,  (int) Video.Quality);
        WriteInteger(Offset.AudioQuality,          (int) Audio.Quality);
        WriteInteger(Offset.AudioVariety,          (int) Audio.Variety);
        WriteInteger(Offset.NetworkConnectionType, (int) Network.Connection);

        /**
         * The following values are values which can have any integer (within the limits of the data types, of course).
         */

        WriteInteger(Offset.VideoResolutionWidth,  Video.Resolution.Width);
        WriteInteger(Offset.VideoResolutionHeight, Video.Resolution.Height);
        WriteInteger(Offset.NetworkPortServer,     Network.Port.Server);
        WriteInteger(Offset.NetworkPortClient,     Network.Port.Client);

        WriteByte(Offset.VideoRefreshRate,           Video.RefreshRate);
        WriteByte(Offset.VideoMiscellaneousGamma,    Video.Gamma);
        WriteByte(Offset.MouseSensitivityHorizontal, Mouse.Sensitivity.Horizontal);
        WriteByte(Offset.MouseSensitivityVertical,   Mouse.Sensitivity.Vertical);
        WriteByte(Offset.AudioVolumeMaster,          Audio.Volume.Master);
        WriteByte(Offset.AudioVolumeEffects,         Audio.Volume.Effects);
        WriteByte(Offset.AudioVolumeMusic,           Audio.Volume.Music);

        /*
         * As for the boolean values, we convert them behind the scene to their integer equivalents -- 1 and 0 for true
         * and false, respectively.
         */

        WriteBoolean(Offset.MouseInvertVerticalAxis, Mouse.InvertVerticalAxis);
        WriteBoolean(Offset.VideoEffectsSpecular,    Video.Effects.Specular);
        WriteBoolean(Offset.VideoEffectsShadows,     Video.Effects.Shadows);
        WriteBoolean(Offset.VideoEffectsDecals,      Video.Effects.Decals);
        WriteBoolean(Offset.AudioEAX,                Audio.EAX);
        WriteBoolean(Offset.AudioHWA,                Audio.HWA);

        /**
         * Mapping is conducted by writing values at offsets, where values = actions and offsets = inputs.
         */

        foreach (var offset in Enum.GetValues(typeof(Button)))
        {
          Debug("Nulling input - " + offset);

          ms.Position = (int) offset;
          bw.Write(0x7F);
        }

        foreach (var mapping in Input.Mapping)
        {
          var value  = (byte) mapping.Key;  /* action */
          var offset = (int) mapping.Value; /* button */

          Debug("Assigning input to action - " + mapping.Key + " -> " + mapping.Value);

          ms.Position = offset;
          bw.Write(value);
        }

        /**
         * The layout of the blam.sav is, in a nutshell:
         *
         * [0x0000 - 0x1005] [0x1FFC - 0x2000]
         *         |                 |
         *         |                 + - hash data (4 bytes)
         *         + ------------------- main data (8188 bytes)
         *
         * By ...
         * 
         * 1.   truncating the last four bytes (the hash) from the memory stream; then
         * 2.   calculating the hash for the main data (i.e. remaining bytes); then
         * 3.   appending it to the memory stream (i.e. replacing the old hash) ...
         *
         * ... we can write the contents to filesystem and expect HCE to accept both the data and the new hash.
         */

        Debug("Truncating CRC32 checksum from memory stream");

        ms.SetLength(ms.Length - 4);

        Debug("Calculating new CRC32 checksum");

        var hash = GetHash(ms.ToArray());

        Debug("New CRC32 hash - 0x" + BitConverter.ToString(hash).Replace("-", string.Empty));

        ms.SetLength(ms.Length + 4);
        ms.Position = (int) Offset.BinaryCrc32Hash;
        bw.Write(hash);

        Debug("Clearing contents of the profile filesystem binary");

        fs.SetLength(0);

        Debug("Copying profile data in memory to the binary file");

        ms.Position = 0;
        ms.CopyTo(fs);

        Info("Saved profile data to the binary on the filesystem");

        /**
         * This method returns a forged CRC-32 hash which can be written to the end of the blam.sav binary. This allows
         * the binary to be considered valid by HCE. By forged hash, we refer to the bitwise complement of a CRC-32 hash
         * of the blam.sav data.
         */

        byte[] GetHash(byte[] data)
        {
          /**
           * This look-up table has been generated from the standard 0xEDB88320 polynomial, which results in hashes that
           * HCE deems valid. The aforementioned polynomial is the reversed equivalent of 0x04C11DB7, and is used, well,
           * everywhere!
           */

          var crcTable = new uint[]
          {
            0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3, 0x0EDB8832,
            0x79DCB8A4, 0xE0D5E91E, 0x97D2D988, 0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91, 0x1DB71064, 0x6AB020F2,
            0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 0x136C9856, 0x646BA8C0, 0xFD62F97A,
            0x8A65C9EC, 0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172,
            0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3,
            0x45DF5C75, 0xDCD60DCF, 0xABD13D59, 0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423,
            0xCFBA9599, 0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924, 0x2F6F7C87, 0x58684C11, 0xC1611DAB,
            0xB6662D3D, 0x76DC4190, 0x01DB7106, 0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433,
            0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01, 0x6B6B51F4,
            0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950,
            0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65, 0x4DB26158, 0x3AB551CE, 0xA3BC0074,
            0xD4BB30E2, 0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0,
            0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9, 0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086, 0x5768B525,
            0x206F85B3, 0xB966D409, 0xCE61E49F, 0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81,
            0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x04DB2615,
            0x73DC1683, 0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
            0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7, 0xFED41B76,
            0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5, 0xD6D6A3E8, 0xA1D1937E,
            0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B, 0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6,
            0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79, 0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236,
            0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7,
            0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D, 0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A, 0x9C0906A9, 0xEB0E363F,
            0x72076785, 0x05005713, 0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38, 0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7,
            0x0BDBDF21, 0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777,
            0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45, 0xA00AE278,
            0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC,
            0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9, 0xBDBDF21C, 0xCABAC28A, 0x53B39330,
            0x24B4A3A6, 0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94,
            0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
          };

          /**
           * With the available data, we conduct a basic cyclic redundancy check operation on it, using the
           * aforementioned look-up table. This provides us with the CRC-32 for the main data in the blam.sav binary.
           *
           * However, possibly for obfuscation, HCE stores the complement (bitwise NOT) of the hash. This requires us to
           * flip each bit in the entire hash. Once we've done that, we are left with the desired hash that can be
           * stored in the blam.sav binary.
           */

          var hashData = BitConverter.GetBytes(~data.Aggregate(0xFFFFFFFF, (checksumRegister, currentByte) =>
            crcTable[(checksumRegister & 0xFF) ^ Convert.ToByte(currentByte)] ^ (checksumRegister >> 8)));

          for (var i = 0; i < hashData.Length; i++)
            hashData[i] = (byte) ~hashData[i];

          return hashData;
        }
      }
    }

    /// <summary>
    ///   Loads object state from the inbound file.
    /// </summary>
    public void Load()
    {
      using (var reader = new BinaryReader(System.IO.File.Open(Path, FileMode.Open)))
      {
        bool GetBoolean(Offset offset)
        {
          return GetByte(offset) == 1;
        }

        ushort GetShort(Offset offset)
        {
          reader.BaseStream.Seek((int) offset, SeekOrigin.Begin);
          return reader.ReadUInt16();
        }

        byte GetByte(Offset offset)
        {
          reader.BaseStream.Seek((int) offset, SeekOrigin.Begin);
          return reader.ReadByte();
        }

        byte[] GetBytes(Offset offset, int count)
        {
          reader.BaseStream.Seek((int) offset, SeekOrigin.Begin);
          return reader.ReadBytes(count);
        }

        Details.Name = Encoding.Unicode.GetString(GetBytes(Offset.ProfileName, 22)).TrimEnd('\0');

        Details.Colour               = (ColourOptions) GetByte(Offset.ProfileColour);
        Video.FrameRate              = (VideoFrameRate) GetByte(Offset.VideoFrameRate);
        Video.Particles              = (VideoParticles) GetByte(Offset.VideoQualityParticles);
        Video.Quality                = (VideoQuality) GetByte(Offset.VideoQualityTextures);
        Audio.Variety                = (AudioVariety) GetByte(Offset.AudioVariety);
        Network.Connection           = (NetworkConnection) GetByte(Offset.NetworkConnectionType);
        Audio.Quality                = (AudioQuality) GetByte(Offset.AudioQuality);
        Audio.Variety                = (AudioVariety) GetByte(Offset.AudioVariety);
        Mouse.Sensitivity.Horizontal = GetByte(Offset.MouseSensitivityHorizontal);
        Mouse.Sensitivity.Vertical   = GetByte(Offset.MouseSensitivityVertical);
        Video.Resolution.Width       = GetShort(Offset.VideoResolutionWidth);
        Video.Resolution.Height      = GetShort(Offset.VideoResolutionHeight);
        Video.RefreshRate            = GetByte(Offset.VideoRefreshRate);
        Video.Gamma                  = GetByte(Offset.VideoMiscellaneousGamma);
        Audio.Volume.Master          = GetByte(Offset.AudioVolumeMaster);
        Audio.Volume.Effects         = GetByte(Offset.AudioVolumeEffects);
        Audio.Volume.Music           = GetByte(Offset.AudioVolumeMusic);
        Network.Port.Server          = GetShort(Offset.NetworkPortServer);
        Network.Port.Client          = GetShort(Offset.NetworkPortClient);
        Mouse.InvertVerticalAxis     = GetBoolean(Offset.MouseInvertVerticalAxis);
        Video.Effects.Specular       = GetBoolean(Offset.VideoEffectsSpecular);
        Video.Effects.Shadows        = GetBoolean(Offset.VideoEffectsShadows);
        Video.Effects.Decals         = GetBoolean(Offset.VideoEffectsDecals);
        Audio.EAX                    = GetBoolean(Offset.AudioEAX);
        Audio.HWA                    = GetBoolean(Offset.AudioHWA);

        Input.Mapping = new Dictionary<ProfileInput.Action, Button>();

        foreach (var button in Enum.GetValues(typeof(Button)))
        {
          reader.BaseStream.Seek((int) button, SeekOrigin.Begin);

          var key   = (ProfileInput.Action) reader.ReadByte();
          var value = (Button) button;

          if (!Input.Mapping.ContainsKey(key))
            Input.Mapping.Add(key, value);
        }

        if ((int) Details.Colour == 0xFF)
          Details.Colour = ColourOptions.White;

        Info("Profile deserialisation routine is complete");
      }
    }

    /// <summary>
    ///   Returns object representing the HCE profile detected on the filesystem.
    /// </summary>
    /// <returns>
    ///   Object representing the HCE profile detected on the filesystem.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    ///   lastprof.txt does not exist.
    ///   - or -
    ///   blam.sav does not exist.
    /// </exception>
    public static Profile Detect()
    {
      return Detect(Paths.HCE.Directory);
    }

    /// <summary>
    ///   Returns object representing the HCE profile detected on the filesystem.
    /// </summary>
    /// <returns>
    ///   Object representing the HCE profile detected on the filesystem.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    ///   lastprof.txt does not exist.
    ///   - or -
    ///   blam.sav does not exist.
    /// </exception>
    public static Profile Detect(string directory)
    {
      var lastprof = (LastProfile) Custom.LastProfile(directory);

      if (!lastprof.Exists())
        throw new FileNotFoundException("Cannot detect profile - lastprof.txt does not exist.");

      lastprof.Load();

      var profile = (Profile) Custom.Profile(directory, lastprof.Profile);

      if (!profile.Exists())
        throw new FileNotFoundException("Cannot load detected profile - its blam.sav does not exist.");

      profile.Load();

      return profile;
    }

    /// <summary>
    ///   Returns a list of profiles representing the blam.sav files found in the specified directory.
    /// </summary>
    /// <param name="directory">
    ///   Directory to look for.
    /// </param>
    /// <returns>
    ///   List of Profile instances.
    /// </returns>
    /// <exception cref="DirectoryNotFoundException">
    ///   Provided profiles directory does not exist.
    /// </exception>
    public static List<Profile> List(string directory)
    {
      if (!Directory.Exists(directory))
        throw new DirectoryNotFoundException("Provided profiles directory does not exist.");

      var profiles = new List<Profile>();

      foreach (var current in Directory.GetFiles(directory, "blam.sav", AllDirectories))
      {
        var profile = (Profile) current;
        profile.Load();
        profiles.Add(profile);
      }

      return profiles;
    }

    /// <summary>
    ///   Returns a list of profiles representing the blam.sav files found in the specified directory.
    /// </summary>
    /// <returns>
    ///   List of Profile instances.
    /// </returns>
    public static List<Profile> List()
    {
      return List(Paths.HCE.Profiles);
    }

    /// <summary>
    ///   Represents the inbound object as a string.
    /// </summary>
    /// <param name="profile">
    ///   Object to represent as string.
    /// </param>
    /// <returns>
    ///   String representation of the inbound object.
    /// </returns>
    public static implicit operator string(Profile profile)
    {
      return profile.Path;
    }

    /// <summary>
    ///   Represents the inbound string as an object.
    /// </summary>
    /// <param name="path">
    ///   String to represent as object.
    /// </param>
    /// <returns>
    ///   Object representation of the inbound string.
    /// </returns>
    public static explicit operator Profile(string path)
    {
      return new Profile
      {
        Path = path
      };
    }

    /// <summary>
    ///   Offsets for the data stored in the blam.sav binary.
    /// </summary>
    private enum Offset
    {
      ProfileName                = 0x0002,
      ProfileColour              = 0x011A,
      MouseInvertVerticalAxis    = 0x012F,
      MouseSensitivityHorizontal = 0x0954,
      MouseSensitivityVertical   = 0x0955,
      VideoResolutionWidth       = 0x0A68,
      VideoResolutionHeight      = 0x0A6A,
      VideoRefreshRate           = 0x0A6C,
      VideoFrameRate             = 0x0A6F,
      VideoEffectsSpecular       = 0x0A70,
      VideoEffectsShadows        = 0x0A71,
      VideoEffectsDecals         = 0x0A72,
      VideoQualityParticles      = 0x0A73,
      VideoQualityTextures       = 0x0A74,
      VideoMiscellaneousGamma    = 0x0A76,
      AudioVolumeMaster          = 0x0B78,
      AudioVolumeEffects         = 0x0B79,
      AudioVolumeMusic           = 0x0B7A,
      AudioEAX                   = 0x0B7B,
      AudioHWA                   = 0x0B7C,
      AudioQuality               = 0x0B7D,
      AudioVariety               = 0x0B7F,
      NetworkConnectionType      = 0x0FC0,
      NetworkPortServer          = 0x1002,
      NetworkPortClient          = 0x1004,
      BinaryCrc32Hash            = 0x1FFC
    }

    public class ProfileDetails
    {
      public enum ColourOptions
      {
        White  = 0x00, /* universal ui => snow */
        Black  = 0x01, /* universal ui => black */
        Red    = 0x02, /* universal ui => crimson */
        Blue   = 0x03, /* universal ui => blue */
        Gray   = 0x04, /* universal ui => steel */
        Yellow = 0x05, /* universal ui => gold */
        Green  = 0x06, /* universal ui => green */
        Pink   = 0x07, /* universal ui => rose */
        Purple = 0x0A, /* universal ui => violet */
        Cyan   = 0x0B, /* universal ui => cyan */
        Cobalt = 0x0C, /* universal ui => cobalt */
        Orange = 0x0D, /* universal ui => orange */
        Teal   = 0x0E, /* universal ui => aqua */
        Sage   = 0x0F, /* universal ui => sage */
        Brown  = 0x10, /* universal ui => brown */
        Tan    = 0x11, /* universal ui => tan */
        Maroon = 0x14, /* universal ui => maroon */
        Salmon = 0x15  /* universal ui => peach */
      }

      public string        Name   { get; set; } = "New001";            /* default value */
      public ColourOptions Colour { get; set; } = ColourOptions.White; /* default value */
    }

    public class ProfileVideo
    {
      public enum VideoFrameRate
      {
        VsyncOff = 0x00,
        VsyncOn  = 0x01,
        Fps30    = 0x02
      }

      public enum VideoParticles
      {
        Off  = 0x00,
        Low  = 0x01,
        High = 0x02
      }

      public enum VideoQuality
      {
        Low    = 0x00,
        Medium = 0x01,
        High   = 0x02
      }

      public VideoResolution Resolution  { get; set; } = new VideoResolution();
      public VideoEffects    Effects     { get; set; } = new VideoEffects();
      public byte            RefreshRate { get; set; } = 60;                   /* default value */
      public byte            Gamma       { get; set; }                         /* unknown value */
      public VideoFrameRate  FrameRate   { get; set; } = VideoFrameRate.Fps30; /* default value */
      public VideoParticles  Particles   { get; set; } = VideoParticles.High;  /* default value */
      public VideoQuality    Quality     { get; set; } = VideoQuality.High;    /* default value */

      public class VideoResolution
      {
        public ushort Width  { get; set; } = 800; /* default value */
        public ushort Height { get; set; } = 600; /* default value */
      }

      public class VideoEffects
      {
        public bool Specular { get; set; } = true; /* default value */
        public bool Shadows  { get; set; } = true; /* default value */
        public bool Decals   { get; set; } = true; /* default value */
      }
    }

    public class ProfileAudio
    {
      public enum AudioQuality
      {
        Low    = 0x00,
        Normal = 0x01,
        High   = 0x02
      }

      public enum AudioVariety
      {
        Low    = 0x00,
        Medium = 0x01,
        High   = 0x02
      }

      public AudioVolume  Volume  { get; set; } = new AudioVolume();
      public AudioQuality Quality { get; set; } = AudioQuality.Normal; /* default value */
      public AudioVariety Variety { get; set; } = AudioVariety.High;   /* default value */
      public bool         EAX     { get; set; }
      public bool         HWA     { get; set; }

      public class AudioVolume
      {
        public byte Effects { get; set; } = 10; /* default value */
        public byte Master  { get; set; } = 10; /* default value */
        public byte Music   { get; set; } = 6;  /* default value */
      }
    }

    public class ProfileMouse
    {
      public bool             InvertVerticalAxis { get; set; } /* default value */
      public MouseSensitivity Sensitivity        { get; set; } = new MouseSensitivity();

      public class MouseSensitivity
      {
        public byte Horizontal { get; set; } = 3; /* default value */
        public byte Vertical   { get; set; } = 3; /* default value */
      }
    }

    public class ProfileNetwork
    {
      public enum NetworkConnection
      {
        DialUp     = 0x00,
        DslLow     = 0x01,
        DslAverage = 0x02,
        DslHigh    = 0x03,
        Lan        = 0x04
      }

      public NetworkConnection Connection { get; set; } = NetworkConnection.Lan; /* default value */
      public NetworkPort       Port       { get; set; } = new NetworkPort();

      public class NetworkPort
      {
        public ushort Server { get; set; } = 2302; /* default value */
        public ushort Client { get; set; } = 2303; /* default value */
      }
    }

    public class ProfileInput
    {
      public enum Action
      {
        MoveForward     = 0x13, /* movement */
        MoveBackward    = 0x14, /* movement */
        MoveLeft        = 0x15, /* movement */
        MoveRight       = 0x16, /* movement */
        LookUp          = 0x17, /* movement */
        LookDown        = 0x18, /* movement */
        LookLeft        = 0x19, /* movement */
        LookRight       = 0x1A, /* movement */
        FireWeapon      = 0x07, /* combat   */
        ThrowGrenade    = 0x06, /* combat   */
        SwitchGrenade   = 0x01, /* combat   */
        SwitchWeapon    = 0x03, /* combat   */
        Reload          = 0x0D, /* combat   */
        MeleeAttack     = 0x04, /* combat   */
        ExchangeWeapon  = 0x0E, /* combat   */
        Jump            = 0x00, /* actions  */
        Crouch          = 0x0A, /* actions  */
        Flashlight      = 0x05, /* actions  */
        ScopeZoom       = 0x0B, /* actions  */
        Action          = 0x02, /* actions  */
        MenuAccept      = 0xFF, /* misc.    */
        MenuBack        = 0xFF, /* misc.    */
        Say             = 0x0F, /* misc.    */
        SayToTeam       = 0x10, /* misc.    */
        SayToVehicle    = 0x11, /* misc.    */
        ShowScores      = 0x0C, /* misc.    */
        ShowRules       = 0x1B, /* misc.    */
        ShowPlayerNames = 0x1C  /* misc.    */
      }

      public enum Button
      {
        DPU   = 0x53A, /* directional - up                */
        DPD   = 0x542, /* directional - down              */
        DPL   = 0x546, /* directional - left              */
        DPR   = 0x53E, /* directional - right             */
        LSU   = 0x33C, /* analogue - left stick - up      */
        LSD   = 0x33A, /* analogue - left stick - down    */
        LSL   = 0x340, /* analogue - left stick - left    */
        LSR   = 0x33E, /* analogue - left stick - right   */
        LSM   = 0x23A, /* analogue - left stick - middle  */
        RSU   = 0x344, /* analogue - right stick - up     */
        RSD   = 0x342, /* analogue - right stick - down   */
        RSL   = 0x348, /* analogue - right stick - left   */
        RSR   = 0x346, /* analogue - right stick - right  */
        RSM   = 0x23C, /* analogue - right stick - middle */
        LB    = 0x232, /* shoulder - bumper - left        */
        RB    = 0x234, /* shoulder - bumper - right       */
        LT    = 0x34A, /* shoulder - trigger - left       */
        RT    = 0x34C, /* shoulder - trigger - right      */
        A     = 0x22A, /* face - button a                 */
        B     = 0x22C, /* face - button b                 */
        X     = 0x22E, /* face - button x                 */
        Y     = 0x230, /* face - button y                 */
        Start = 0x238, /* home - start                    */
        Back  = 0x236  /* home - back                     */
      }

      public Dictionary<Action, Button> Mapping = new Dictionary<Action, Button>();
    }
  }
}