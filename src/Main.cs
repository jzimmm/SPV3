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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using HXE;
using SPV3.Annotations;
using static System.IO.File;
using static System.IO.Path;
using static System.Reflection.Assembly;

namespace SPV3
{
  public class Main
  {
    public MainVersion Version { get; set; } = new MainVersion();
    public MainUpdate  Update  { get; set; } = new MainUpdate();
    public MainError   Error   { get; set; } = new MainError();
    public MainInstall Install { get; set; } = new MainInstall();
    public MainLoad    Load    { get; set; } = new MainLoad();

    public void Initialise()
    {
      Version.Initialise();

      if (!Exists(Paths.HCE.Executable))
      {
        Load.Visibility = Visibility.Collapsed;

        if (Exists(Combine("data", Paths.Manifest)))
          Install.Visibility = Visibility.Visible;
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
        Error.Content    = "Update error - " + e.Message;
      }
    }

    public class MainUpdate : INotifyPropertyChanged
    {
      private Visibility _visibility = Visibility.Collapsed;
      private string     _content;
      private string     _address;

      public Visibility Visibility
      {
        get => _visibility;
        set
        {
          if (value == _visibility) return;
          _visibility = value;
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

      public void Initialise()
      {
        try
        {
          Content    = "Update 10 available!";
          Visibility = Visibility.Visible;
        }
        catch (Exception)
        {
          Visibility = Visibility.Collapsed;
          throw;
        }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    public class MainVersion : INotifyPropertyChanged
    {
      private int    _current;
      private string _content;
      private string _address;

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

      public void Initialise()
      {
        var versionMajor = GetEntryAssembly()?.GetName().Version.Major;

        if (versionMajor == null) return;

        Current = (int) versionMajor;
        Content = $"Version {Current:D4}";
        Address = $"https://github.com/yumiris/SPV3/tree/build-{Current:D4}";
      }

      public event PropertyChangedEventHandler PropertyChanged;

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    public class MainError : INotifyPropertyChanged
    {
      private Visibility _visibility = Visibility.Collapsed;
      private string     _content;

      public Visibility Visibility
      {
        get => _visibility;
        set
        {
          if (value == _visibility) return;
          _visibility = value;
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

      public event PropertyChangedEventHandler PropertyChanged;

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    public class MainLoad : INotifyPropertyChanged
    {
      private Visibility _visibility = Visibility.Collapsed;

      public Visibility Visibility
      {
        get => _visibility;
        set
        {
          if (value == _visibility) return;
          _visibility = value;
          OnPropertyChanged();
        }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    public class MainInstall : INotifyPropertyChanged
    {
      private Visibility _visibility = Visibility.Collapsed;

      public Visibility Visibility
      {
        get => _visibility;
        set
        {
          if (value == _visibility) return;
          _visibility = value;
          OnPropertyChanged();
        }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }
}