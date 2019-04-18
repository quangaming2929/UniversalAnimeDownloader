﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using UADAPI;
using UniversalAnimeDownloader.MediaPlayer;
using UniversalAnimeDownloader.UcContentPages;
using UniversalAnimeDownloader.ViewModels;

namespace UniversalAnimeDownloader
{
    public class MiscClass
    {
        public static NavigationTrack NavigationHelper { get; set; } = new NavigationTrack();
        public static List<string> PresetQuality { get; set; } = new List<string>
        {
            "144p", "288p", "360p", "480p", "720p", "1080p", "1440p", "2160p", "Best possible", "Worse possible"
        };

        public static event EventHandler<SearchEventArgs> UserSearched;
        public static void OnUserSearched(object sender, string searchKeyword) => UserSearched?.Invoke(sender, new SearchEventArgs(searchKeyword));
        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);

                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }

            return null;
        }
        public static IList<T> FindChildren<T>(DependencyObject element) where T : FrameworkElement
        {
            List<T> retval = new List<T>();
            for (int counter = 0; counter < VisualTreeHelper.GetChildrenCount(element); counter++)
            {
                FrameworkElement toadd = VisualTreeHelper.GetChild(element, counter) as FrameworkElement;
                if (toadd != null)
                {
                    T correctlyTyped = toadd as T;
                    if (correctlyTyped != null)
                    {
                        retval.Add(correctlyTyped);
                    }
                    else
                    {
                        retval.AddRange(FindChildren<T>(toadd));
                    }
                }
            }
            return retval;
        }
        public static T FindParent<T>(DependencyObject element) where T : FrameworkElement
        {
            FrameworkElement parent = VisualTreeHelper.GetParent(element) as FrameworkElement;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }
                return FindParent<T>(parent);
            }
            return null;
        }
        public static string AddHtmlColorBody(string content, Color color)
        {
            string rgbValue = $"rgb({color.R}, {color.G}, {color.B})";
            return $"<body style=\"color: {rgbValue}\">" + content + " </body>";
        }
        public static void FadeInAnimation(UIElement target, Duration duration, bool removePreviousAnimation = true, EventHandler completedCallback = null)
        {
            DoubleAnimation animation = new DoubleAnimation(1, duration);
            if (completedCallback != null)
                animation.Completed += completedCallback;
            if (removePreviousAnimation)
                target.BeginAnimation(UIElement.OpacityProperty, null);
            target.BeginAnimation(UIElement.OpacityProperty, animation);
        }
        public static void FadeOutAnimation(UIElement target, Duration duration, bool removePreviousAnimation = true)
        {
            DoubleAnimation animation = new DoubleAnimation(0, duration);
            if (removePreviousAnimation)
                target.BeginAnimation(UIElement.OpacityProperty, null);
            target.BeginAnimation(UIElement.OpacityProperty, animation);
        }
        public static TimeSpan MutiplyTimeSpan(TimeSpan a, double b)
        {
            double milli = a.TotalMilliseconds * b;
            return TimeSpan.FromMilliseconds(milli);
        }
        public static double GetTimeSpanRatio(TimeSpan a, TimeSpan b) => a.TotalMilliseconds / b.TotalMilliseconds;

        public static void CancelCloseWindow(object sender, CancelEventArgs e) => e.Cancel = true;

        static MiscClass()
        {
            NavigationHelper.AddNavigationHistory(0);
        }
    }

    public class UADMediaPlayerHelper
    {
        private static MainWindowViewModel _Ins;
        private static UADMediaPlayer _Player;

        public static void Play(AnimeSeriesInfo info, int index = 0)
        {
            NullCheck();
            _Player.Playlist = info;
            if (index != 0)
                _Player.PlayIndex = index;

            _Player.VM.UpdateBindings();
            (_Player.Parent as Grid).Opacity = 0;
            _Player.Focus();
            _Ins.UADMediaPlayerVisibility = Visibility.Visible;
            MiscClass.FadeInAnimation(_Player.Parent as Grid, TimeSpan.FromSeconds(.5), true, new EventHandler(PlayMedia));
            _Player.Focus();
        }

        private static void PlayMedia(object sender, EventArgs e)
        {
            (_Player.Parent as Grid).IsHitTestVisible = true;
            _Player.mediaPlayer.Play();
            _Player.Focus();
        }

        private static void NullCheck()
        {
            if (_Ins == null)
                _Ins = Application.Current.FindResource("MainWindowViewModel") as MainWindowViewModel;
            if (_Player == null)
                _Player = MiscClass.FindVisualChild<UADMediaPlayer>(Application.Current.MainWindow) as UADMediaPlayer;
        }
    }

    public class SearchEventArgs : EventArgs
    {
        public SearchEventArgs()
        {

        }

        public SearchEventArgs(string keyword)
        {
            Keyword = keyword;
        }
        public string Keyword { get; set; }
    }
    public class SelectableEpisodeInfo : INotifyPropertyChanged
    {
        private EpisodeInfo _Data;
        public EpisodeInfo Data
        {
            get
            {
                return _Data;
            }
            set
            {
                if (_Data != value)
                {
                    _Data = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                if (_IsSelected != value)
                {
                    _IsSelected = value;
                    OnSelectedChanged();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Just for multibinding
        /// </summary>
        public double Offset { get; set; } = 100;

        public SelectableEpisodeInfo()
        {

        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnSelectedChanged() => SelectedIndexChanged?.Invoke(this, EventArgs.Empty);

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EventArgs> SelectedIndexChanged;
    }
    public class NavigationTrack
    {
        private int _Position = -1;
        public List<int> History { get; } = new List<int>();
        public bool CanGoBack { get => _Position > 0; }
        public bool CanGoForward { get => _Position < History.Count - 1; }
        public int Current { get; private set; }

        public void RemoveFromAt(int index)
        {
            while (index < History.Count)
            {
                History.RemoveAt(index);
            }
        }

        public void AddNavigationHistory(int pageIndex)
        {
            _Position++;
            RemoveFromAt(_Position);
            History.Add(pageIndex);
            Current = pageIndex;
        }

        public int Back()
        {
            _Position--;
            Current = History[_Position];
            return History[_Position];
        }

        public int Forward()
        {
            _Position++;
            Current = History[_Position];
            return History[_Position];
        }

        public void Reset()
        {
            History.Clear();
            Current = -1;
            _Position = -1;
        }
    }

    public static class PresetColors
    {
        public static Color[] Colors = new Color[]
        {
            Color.FromRgb(244,43,36),
            Color.FromRgb(0xE9,0x1E,63),
            Color.FromRgb(0x9C,27,0xB0),
            Color.FromRgb(67,0x3A,0xB7),
            Color.FromRgb(0x3F,51,0xB5),
            Color.FromRgb(21,96,0xF3),
            Color.FromRgb(21,96,0xF3),
            Color.FromRgb(00,0xBC,0xD4),
            Color.FromRgb(00,96,88),
            Color.FromRgb(0x4C,0xAF,50),
            Color.FromRgb(0x8B,0xC3,0x4A),
            Color.FromRgb(0xCD,0xDC,39),
            Color.FromRgb(0xFF,0xEB,0x3B),
            Color.FromRgb(0xFF,0xC1,07),
            Color.FromRgb(0xFF,98,00),
            Color.FromRgb(0xFF,57,22),
            Color.FromRgb(79,55,48),
            Color.FromRgb(0x9E,0x9E,0x9E),
            Color.FromRgb(60,0x7D,0x8B),
        };

        private static Random rand = new Random();

        public static Color GetRandomColor() => Colors[rand.Next(0, Colors.Length)];
    }
    
}
