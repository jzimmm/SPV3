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

using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using SPV3.Annotations;
using static System.IO.FileAccess;
using static System.IO.FileMode;
using static System.Windows.Forms.Screen;

namespace SPV3
{
  public class Configuration : INotifyPropertyChanged
  {
    private ushort _height = (ushort) PrimaryScreen.Bounds.Height;
    private ushort _width  = (ushort) PrimaryScreen.Bounds.Width;
    private bool   _window;

    public bool Window
    {
      get => _window;
      set
      {
        if (value == _window) return;
        _window = value;
        OnPropertyChanged();
      }
    }

    public ushort Width
    {
      get => _width;
      set
      {
        if (value == _width) return;
        _width = value;
        OnPropertyChanged();
      }
    }

    public ushort Height
    {
      get => _height;
      set
      {
        if (value == _height) return;
        _height = value;
        OnPropertyChanged();
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void Save()
    {
      using (var fs = new FileStream(Paths.Configuration, Create, Write))
      using (var ms = new MemoryStream(256))
      using (var bw = new BinaryWriter(ms))
      {
        /* signature */
        {
          bw.Write(Encoding.Unicode.GetBytes("~yumiris"));
        }

        /* padding */
        {
          bw.Write(new byte[16 - ms.Position]);
        }

        /* video */
        {
          bw.Write(Width);
          bw.Write(Height);
          bw.Write(Window);
        }

        /* padding */
        {
          bw.Write(new byte[256 - ms.Position]);
        }

        ms.Position = 0;
        ms.CopyTo(fs);
      }
    }

    public void Load()
    {
      using (var fs = new FileStream(Paths.Configuration, Open, Read))
      using (var ms = new MemoryStream(256))
      using (var br = new BinaryReader(ms))
      {
        fs.CopyTo(ms);
        ms.Position = 0;

        /* padding */
        {
          ms.Position += 16 - ms.Position;
        }

        /* video */
        {
          Width  = br.ReadUInt16();
          Height = br.ReadUInt16();
          Window = br.ReadBoolean();
        }
      }
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}