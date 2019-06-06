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
using System.Reflection;
using System.Runtime.CompilerServices;
using SPV3.Annotations;

namespace SPV3
{
  public partial class Main
  {
    public class MainVersion : INotifyPropertyChanged
    {
      private string _address;
      private string _content;
      private int    _current;

      public int Current
      {
        get => _current;
        set
        {
          if (value == _current) return;
          _current = value;
          OnPropertyChanged();
        }
      }

      public string Content
      {
        get => _content;
        set
        {
          if (value == _content) return;
          _content = value;
          OnPropertyChanged();
        }
      }

      public string Address
      {
        get => _address;
        set
        {
          if (value == _address) return;
          _address = value;
          OnPropertyChanged();
        }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      public void Initialise()
      {
        var versionMajor = Assembly.GetEntryAssembly()?.GetName().Version.Major;

        if (versionMajor == null) return;

        Current = (int) versionMajor;
        Content = $"Version {Current:D4}";
        Address = $"https://github.com/yumiris/SPV3/tree/build-{Current:D4}";
      }

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }
}