﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using UADAPI;
using UniversalAnimeDownloader.MediaPlayer;
using UniversalAnimeDownloader.UADSettingsPortal;
using UniversalAnimeDownloader.UcContentPages;
using UniversalAnimeDownloader.ViewModels;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace UniversalAnimeDownloader
{
    public class MiscClass
    {
        public static string GlobalVersion = "0.9.0";
        public static NavigationTrack NavigationHelper { get; set; } = new NavigationTrack();
        public static List<string> PresetQuality { get; set; } = new List<string>
        {
            "144p", "288p", "360p", "480p", "720p", "1080p", "1440p", "2160p", "Best possible", "Worse possible"
        };
        public static List<string> MediaPlayerOptions { get; set; } = new List<string>
        {
            "External Media Player", "UAD Media Player"
        };
        public static string[] StretchString { get => Enum.GetNames(typeof(Stretch)); }
        public static string[] BitmapScalingModeString { get => Enum.GetNames(typeof(BitmapScalingMode)); }

        public static JsonSerializerSettings IgnoreConverterErrorJson = new JsonSerializerSettings()
        {
            Error = new EventHandler<ErrorEventArgs>((s, e) => e.ErrorContext.Handled = true),
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
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

        public static BitmapImage SaveFrameworkElementToPng(FrameworkElement frameworkElement, int width, int height, string filePath)
        {
            BitmapImage bitmapImage = VisualToBitmapImage(frameworkElement);

            SaveImage(bitmapImage, width, height, filePath);
            return bitmapImage;
        }

        public static BitmapImage VisualToBitmapImage(FrameworkElement frameworkElement)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)frameworkElement.ActualWidth, (int)frameworkElement.ActualHeight, 96d, 96d, PixelFormats.Default);
            rtb.Render(frameworkElement);

            MemoryStream stream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(stream);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public static void SaveImage(BitmapImage sourceImage, int width, int height, string filePath)
        {
            TransformGroup transformGroup = new TransformGroup();
            ScaleTransform scaleTransform = new ScaleTransform();
            scaleTransform.ScaleX = (double)width / sourceImage.PixelWidth;
            scaleTransform.ScaleY = (double)height / sourceImage.PixelHeight;
            transformGroup.Children.Add(scaleTransform);

            DrawingVisual vis = new DrawingVisual();
            DrawingContext cont = vis.RenderOpen();
            cont.PushTransform(transformGroup);
            cont.DrawImage(sourceImage, new Rect(new Size(sourceImage.PixelWidth, sourceImage.PixelHeight)));
            cont.Close();

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
            rtb.Render(vis);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(stream);
                stream.Close();
            }
        }

        public static void CopyDirectory(string source, string destination)
        {
            foreach (string dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(Path.Combine(destination, dir.Substring(source.Length + 1)));
            foreach (string file_name in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                File.Copy(file_name, Path.Combine(destination, file_name.Substring(source.Length + 1)), true);
        }

        public static int FindObservableCollectionIndex<T>(ObservableCollection<T> collection, Func<T, bool> predicate)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if (predicate(collection[i]))
                    return i;
            }

            return -1;
        }

        public static async Task<Tuple<bool, bool, Exception>> CreateCustomAnimeSeries(AnimeSeriesInfo p)
        {
            bool isAnyUnrecogizedPart = false;
            try
            {
                await Task.Run(() =>
                {
                    var SettingsData = (Application.Current.FindResource("Settings") as UADSettingsManager).CurrentSettings;
                    //Create a random object to randomize Anime and Episode ids
                    Random rand = new Random();
                    if (p.AnimeID == 0)
                        p.AnimeID = rand.Next();
                    p.AnimeSeriesSavedDirectory = Path.Combine(SettingsData.AnimeLibraryLocation, $"{p.AnimeID}-{p.Name.RemoveInvalidChar()}");
                    if (!Directory.Exists(p.AnimeSeriesSavedDirectory))
                        Directory.CreateDirectory(p.AnimeSeriesSavedDirectory);
                    p.ManagerFileLocation = Path.Combine(p.AnimeSeriesSavedDirectory, "Manager.json");
                    p.IsCustomSeries = true;
                    var seriesThumbnailInfo = new FileInfo(p.Thumbnail.LocalFile);
                    var newSeriesThumbnailLocation = Path.Combine(p.AnimeSeriesSavedDirectory, $"SeriesThumbnail{seriesThumbnailInfo.Extension}");
                    if (File.Exists(p.Thumbnail.LocalFile))
                    {
                        if (p.Thumbnail.LocalFile != newSeriesThumbnailLocation)
                        {
                            File.Copy(p.Thumbnail.LocalFile, newSeriesThumbnailLocation, true);
                        }
                    }
                        else
                    {
                        isAnyUnrecogizedPart = true;
                    }

                    p.Thumbnail.LocalFile = newSeriesThumbnailLocation;

                    p.IsSelectiveDownload = true;
                    for (int i = 0; i < p.Episodes.Count; i++)
                    {
                        EpisodeInfo item = p.Episodes[i];
                        item.AvailableOffline = true;
                        if (item.EpisodeID == 0)
                            item.EpisodeID = rand.Next();
                        if (item.Index == 0)
                            item.Index = i + 1;

                        var episodeThumbnailInfo = new FileInfo(item.Thumbnail.LocalFile);
                        var newEpisodeThumbnailLocation = Path.Combine(p.AnimeSeriesSavedDirectory, $"{item.EpisodeID}-{item.Index}-Thumbnail{episodeThumbnailInfo.Extension}");
                        if (File.Exists(item.Thumbnail.LocalFile))
                        {
                            if (item.Thumbnail.LocalFile != newEpisodeThumbnailLocation)
                            {
                                File.Copy(item.Thumbnail.LocalFile, newEpisodeThumbnailLocation, true);
                            }
                        }
                        else
                        {
                            isAnyUnrecogizedPart = true;
                        }

                        item.Thumbnail.LocalFile = newEpisodeThumbnailLocation;

                        if (item.FilmSources.ContainsKey(VideoQuality.Quality144p))
                        {
                            var currentFilmSource = item.FilmSources[VideoQuality.Quality144p];
                            var filmSourceInfo = new FileInfo(currentFilmSource.LocalFile);
                            var newFilmSourceLocation = Path.Combine(p.AnimeSeriesSavedDirectory, $"{item.EpisodeID}-{item.Index} {item.Name}{filmSourceInfo.Extension}");
                            if (File.Exists(currentFilmSource.LocalFile))
                            {
                                if (currentFilmSource.LocalFile != newFilmSourceLocation)
                                {
                                    File.Copy(currentFilmSource.LocalFile, newFilmSourceLocation, true);
                                }
                            }
                            else
                            {
                                isAnyUnrecogizedPart = true;
                            }

                            currentFilmSource.LocalFile = newFilmSourceLocation;
                        }
                    }

                    var managerFileContent = JsonConvert.SerializeObject(p);
                    File.WriteAllText(p.ManagerFileLocation, managerFileContent);

                });
                return new Tuple<bool, bool, Exception>(isAnyUnrecogizedPart, false, null);
            }
            catch (Exception e)
            {
                return new Tuple<bool, bool, Exception>(isAnyUnrecogizedPart, true, e);
            }
        }
    }

    public class UADMediaPlayerHelper
    {
        private static MainWindowViewModel _Ins;
        private static UADMediaPlayer _Player;
        private static Popup _NowPlayingPopup;
        public static bool IsOnlineMediaPlayerPlaying = false;

        public static Popup NowPlayingPopup
        {
            get
            {
                if (_NowPlayingPopup == null)
                    NullCheck();
                return _NowPlayingPopup;
            }
        }

        public static UADMediaPlayer MediaPlayer
        {
            get
            {
                if (_Player == null)
                    NullCheck();
                return _Player;
            }
        }

        public static async void Play(AnimeSeriesInfo info, int index = 0, bool isOnline = false)
        {
            if((Application.Current.FindResource("Settings") as UADSettingsManager).CurrentSettings.PreferedPlayer == PlayerType.Embeded)
            {
                NullCheck();
                //To prevent 2 video from playing
                if(_Ins.IsPlayButtonEnable)
                {
                    _Player.mediaPlayer.Stop();
                }

                IsOnlineMediaPlayerPlaying = isOnline;
                _Player.IsPlayOnline = isOnline;

                _Player.Reset();
                _Ins.NowPlayingPlaylist = info;
                _Ins.UADMediaPlayerPlayIndex = index;

                _Player.VM.UpdateBindings();
                (_Player.Parent as Grid).Opacity = 0;
                _Player.Focus();
                _Ins.UADMediaPlayerVisibility = Visibility.Visible;
                
                MiscClass.FadeInAnimation(_Player.Parent as Grid, TimeSpan.FromSeconds(.5), true, new EventHandler((s,e) => 
                {
                    (_Player.Parent as Grid).IsHitTestVisible = true;
                    _Player.Play();
                    _Player.Focus();
                }));
                _Player.Focus();
            }
            else
            {
                var episode = info.Episodes[index];
                if(episode.FilmSources == null)
                {
                    var manager = ApiHelpper.CreateAnimeSeriesManagerObjectByClassName(info.ModInfo.ModTypeString);
                    manager.AttachedAnimeSeriesInfo = info;
                    await manager.GetEpisodes(new List<int>() { episode.EpisodeID });
                }
                var source = episode.FilmSources.Where(p => !string.IsNullOrEmpty(p.Value.LocalFile));

                if (isOnline)
                {
                    var res = await MessageDialog.ShowAsync("Waring!", "Playing online using your broswer will likely be blocked by the server due to referer and origin policy. Do you still want to continue?", MessageDialogButton.YesNoButton);
                    if (res == MessageDialogResult.No)
                        return;
                    try {
                        Process.Start(episode.FilmSources.Last().Value.Url); return;
                    } catch { };
                }
                else
                {
                    if (episode.AvailableOffline)
                    {
                        if (source.Count() != 0)
                            try { Process.Start(source.Last().Value.LocalFile); return; } catch { };
                    }
                    await MessageDialog.ShowAsync("This episode is not avaible offline", "This episode is not avaible offline, you can download it by visiting online version. We also suggest you try our UAD Media Player. It pack with a ton of feature and support play all episode correctly", MessageDialogButton.OKCancelButton);
                }
                
            }
            
        }

        public static async void TogglePlayPause()
        {
            NullCheck();
            if (_Player.IsPlaying)
                await _Player.PlayPauseMedia();
        }

        public static void LockSliderPosition()
        {
            NullCheck();
            _Player.IsSeekSliderLocked = true;
        }

        public static void ChangePositionByProgress(double progress)
        {
            NullCheck();
            _Player.ChangePositionProgress(progress);
        }

        /// <summary>
        /// Change index without binding
        /// </summary>
        /// <param name="index"></param>
        public static void ChangeDirectIndex(int index)
        {
            NullCheck();
            _Player.PlayIndex = index;
        }

        public static void Next()
        {
            NullCheck();
            _Player.Next();
        }

        public static void Previous()
        {
            NullCheck();
            _Player.Previous();
        }

        private static void NullCheck()
        {
            if (_Ins == null)
                _Ins = Application.Current.FindResource("MainWindowViewModel") as MainWindowViewModel;
            if (_Player == null)
                _Player = MiscClass.FindVisualChild<UADMediaPlayer>(Application.Current.MainWindow);
            if (_NowPlayingPopup == null)
                _NowPlayingPopup = Application.Current.MainWindow.FindName("nowPlayingPopup") as Popup;
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
