﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using UADAPI;
using UniversalAnimeDownloader.UADSettingsPortal;

namespace UniversalAnimeDownloader.ViewModels
{
    class ExploreViewModel : BaseViewModel, IPageContent
    {
        public ICommand NavigateGetMoreCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand ShowAnimeDetailCommand { get; set; }
        public ICommand ReloadCommand { get; set; }

        public IQueryAnimeSeries Querier { get; set; }

        public DelayedObservableCollection<AnimeSeriesInfo> FeaturedAnimeList { get; set; } = new DelayedObservableCollection<AnimeSeriesInfo>();
        public DelayedObservableCollection<AnimeSeriesInfo> SuggestedAnimeList { get; set; } = new DelayedObservableCollection<AnimeSeriesInfo>();
        public ObservableCollection<AnimeSeriesInfo> CarouselAnimeList { get; set; } = new DelayedObservableCollection<AnimeSeriesInfo>();
        public CancellationTokenSource LoadFeaturedAnimeCancelToken { get; set; } = new CancellationTokenSource();
        public CancellationTokenSource LoadSuggestedAnimeCancelToken { get; set; } = new CancellationTokenSource();
        public bool IsLoadedAnime { get; set; }

        private Visibility _OverlayNoInternetVisibility = Visibility.Collapsed;
        public Visibility OverlayNoInternetVisibility
        {
            get
            {
                return _OverlayNoInternetVisibility;
            }
            set
            {
                if (_OverlayNoInternetVisibility != value)
                {
                    _OverlayNoInternetVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility _OverlayNoModInstalledVisibility = Visibility.Collapsed;
        public Visibility OverlayNoModInstalledVisibility
        {
            get
            {
                return _OverlayNoModInstalledVisibility;
            }
            set
            {
                if (_OverlayNoModInstalledVisibility != value)
                {
                    _OverlayNoModInstalledVisibility = value;
                    OnPropertyChanged();
                }
            }
        }



        public ExploreViewModel()
        {
            LoadedCommand = new RelayCommand<object>(null, Init);
            NavigateGetMoreCommand = new RelayCommand<string>(null, p => (Application.Current.FindResource("MainWindowViewModel") as MainWindowViewModel).NavigateProcess(p));

            ShowAnimeDetailCommand = new RelayCommand<AnimeSeriesInfo>(null, async p =>
             {
                 if (p == null)
                 {
                     await MessageDialog.ShowAsync("Loading in progress...", "We are getting anime series for you, be patient!", MessageDialogButton.OKCancelButton);
                     return;
                 }

                 IAnimeSeriesManager manager = ApiHelpper.CreateAnimeSeriesManagerObjectByClassName(p.ModInfo.ModTypeString);
                 manager.AttachedAnimeSeriesInfo = p;
                 await manager.GetPrototypeEpisodes();
                 (Application.Current.FindResource("AnimeDetailsViewModel") as AnimeDetailsViewModel).CurrentSeries = manager;
                 (Application.Current.FindResource("MainWindowViewModel") as MainWindowViewModel).NavigateProcess("AnimeDetails");
             });

            ReloadCommand = new RelayCommand<object>(null, async p =>
            {
                if (await ApiHelpper.CheckForInternetConnection())
                {
                    await (Application.Current.FindResource("FeaturedAnimeViewModel") as FeaturedAnimeViewModel).LoadFeaturedAnime(0, 50);
                    await (Application.Current.FindResource("AnimeSuggestionViewModel") as AnimeSuggestionViewModel).LoadSuggestedAnime(0, 50);
                    await (Application.Current.FindResource("AllAnimeTabViewModel") as AllAnimeTabViewModel).LoadAnime(0, 50);
                }
            });
        }

        private async void Init(object obj)
        {
            OverlayNoInternetVisibility = await ApiHelpper.CheckForInternetConnection() ? Visibility.Collapsed : Visibility.Visible;

            //If user don't install any mod
            if (ApiHelpper.QueryTypes.Count == 0)
            {
                OverlayNoModInstalledVisibility = Visibility.Visible;
                return;
            }

            Querier = ApiHelpper.CreateQueryAnimeObjectByType(ApiHelpper.QueryTypes[0]);

            InitAnimeList();
        }

        public async void InitAnimeList()
        {
            //await LoadFeaturedAnime(0, 10);
            var featuredVM = Application.Current.FindResource("FeaturedAnimeViewModel") as FeaturedAnimeViewModel;
            if(featuredVM.FeaturedAnimeInfos.Count != 0)
                await FeaturedAnimeList.AddRange(featuredVM.FeaturedAnimeInfos.Where((f, i) => i > 6 && i < 18).ToList(), LoadFeaturedAnimeCancelToken.Token);
            featuredVM.LoadFeaturedAnimeCompleted += async (s, e) =>
            {
                LoadFeaturedAnimeCancelToken?.Cancel();
                LoadFeaturedAnimeCancelToken = new CancellationTokenSource();
                FeaturedAnimeList.RemoveAll();
                CarouselAnimeList.Clear();
                try
                {
                    for (int i = 0; i < 6; i++)
                    {
                        CarouselAnimeList.Add(featuredVM.FeaturedAnimeInfos[i]);
                    }
                    await FeaturedAnimeList.AddRange(featuredVM.FeaturedAnimeInfos.Where((f, i) => i > 6 && i < 18).ToList(), LoadFeaturedAnimeCancelToken.Token);
                }
                catch { }
            };
            var suggestedVM = Application.Current.FindResource("AnimeSuggestionViewModel") as AnimeSuggestionViewModel;
            if(suggestedVM.SuggestedAnimeInfos.Count != 0)
                await SuggestedAnimeList.AddRange(suggestedVM.SuggestedAnimeInfos.Where((f, i) => i < 10).ToList(), LoadSuggestedAnimeCancelToken.Token);
            suggestedVM.LoadSuggestedAnimeCompleted += async (s, e) =>
            {
                LoadSuggestedAnimeCancelToken?.Cancel();
                LoadSuggestedAnimeCancelToken = new CancellationTokenSource();
                FeaturedAnimeList.RemoveAll();
                try
                {
                    await SuggestedAnimeList.AddRange(suggestedVM.SuggestedAnimeInfos.Where((f, i) => i < 10).ToList(), LoadSuggestedAnimeCancelToken.Token);
                }
                catch { }
            };
        }

        public void OnShow()
        {
            if ((Application.Current.FindResource("Settings") as UADSettingsManager).CurrentSettings.IsOnlyLoadWhenHostShow && !IsLoadedAnime)
            {
                IsLoadedAnime = true;
                InitAnimeList();
            }
        }

        public void OnHide()
        {
        }
    }
}
