﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using uadcorelib;
using UniversalAnimeDownloader.Models;
using xNet;

namespace UniversalAnimeDownloader.ViewModel
{
    public class AnimeListViewModel : ViewModelBase
    {
        public ObservableCollection<VuigheGenreModel> VuigheGenres {get;set;}
        private Dispatcher currentDispatcher;
        private bool isHaveConnection;
        public bool IsHaveConnection
        {
            get { return isHaveConnection; }
            set
            {
                if (value != isHaveConnection)
                    isHaveConnection = value;
                OnPropertyChanged("IsHaveConnection");
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (value != isLoading)
                    isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        public AnimeListViewModel(Dispatcher currentDispatcher)
        {
            //Init
            this.currentDispatcher = currentDispatcher;
            VuigheGenres = new ObservableCollection<VuigheGenreModel>();
            IsLoading = true;

            //Get Genre from sources. Do async in case of the internet is slow
            Thread thd = new Thread(QueryGenres);
            thd.IsBackground = true;
            thd.SetApartmentState(ApartmentState.STA);
            thd.Start();

        }

        private void QueryGenres()
        {
            HttpRequest request = new HttpRequest();
            try
            {
                string res = request.Get("https://vuighe.net/anime").ToString();
                Match match = Regex.Match(res, @"(<div class=""genre"").*?(</div>)", RegexOptions.Singleline);
                string matchRes = match.ToString();
                string[] elementCollection = matchRes.Split('\n').Where(query => query.Contains("href")).ToArray();
                foreach (string item in elementCollection)
                {
                    VuigheGenreModel vuigheGenre = new VuigheGenreModel();
                    string hrefElement = BaseLibraryClass.GetElementArrtibute(item, "href");
                    vuigheGenre.Slug = hrefElement.Substring(hrefElement.LastIndexOf('/') + 1);
                    vuigheGenre.Name = BaseLibraryClass.GetElementInline(item);
                    currentDispatcher.Invoke(() => VuigheGenres.Add(vuigheGenre));
                }
            }
            catch(Exception e)
            {
                currentDispatcher.Invoke(() => VuigheGenres.Add(new VuigheGenreModel() { Name = "No Internet Connection", Slug = "no-internet" }));
            }
        }
    }
}