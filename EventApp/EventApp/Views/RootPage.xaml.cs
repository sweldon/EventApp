﻿using EventApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RootPage : MasterDetailPage
    {
        Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();
        public RootPage()
        {
            InitializeComponent();

            MasterBehavior = MasterBehavior.Popover;
         

        }

        public async Task NavigateFromMenu(int id)
        {
            if (!MenuPages.ContainsKey(id))
            {
                switch (id)
                {
                    case (int)MenuItemType.Browse:
                        MenuPages.Add(id, new NavigationPage(new HolidaysPage()));
                        break;
                    case (int)MenuItemType.Trending:
                        MenuPages.Add(id, new NavigationPage(new Trending()));
                        break;
                }
            }

            var newPage = MenuPages[id];

            if (newPage != null && Detail != newPage)
            {
                Detail = newPage;

                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);

                IsPresented = false;
            }
        }
    }
}