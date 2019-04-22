using EventApp.Models;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        RootPage RootPage { get => Application.Current.MainPage as RootPage; }

        List<HomeMenuItem> menuItems;

        public string isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
            set
            {
                if (Settings.IsLoggedIn == value)
                    return;
                Settings.IsLoggedIn = value;
                OnPropertyChanged();
            }
        }

        public string currentUser
        {
            get { return Settings.CurrentUser; }
            set
            {
                if (Settings.CurrentUser == value)
                    return;
                Settings.CurrentUser = value;
                OnPropertyChanged();
            }
        }
       
        public NavigationPage NavigationPage { get; private set; }
        public MenuPage()
        {
            InitializeComponent();

            menuItems = new List<HomeMenuItem>
            {
                new HomeMenuItem {Id = MenuItemType.Notifications, Title="Notifications", MenuImage="alarm.png"},
                new HomeMenuItem {Id = MenuItemType.Holidays, Title="Today", MenuImage="today_icon.png"},
                new HomeMenuItem {Id = MenuItemType.Trending, Title="Trending", MenuImage="trending.png"}
            };

            ListViewMenu.ItemsSource = menuItems;
            //ListViewMenu.SelectedItem = menuItems[0];
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                var id = (int)((HomeMenuItem)e.SelectedItem).Id;
                await RootPage.NavigateFromMenu(id);
            };


            //swipeContainer.Swipe += (sender, e) =>
            //{
            //    switch (e.Direction)
            //    {
            //        case SwipeDirection.Left:
            //            (Application.Current.MainPage as MasterDetailPage).IsPresented = false;
            //            break;
            //    }
            //};

        }

        public async void PromptLogin(object sender, EventArgs e) {
            LoginButton.IsEnabled = false;
            await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            LoginButton.IsEnabled = true;
        }

        public async void LogoutUser(object sender, EventArgs e)
        {

            isLoggedIn = "no";
            LogoutButton.IsVisible = false;
            LoginButton.IsVisible = true;
            UserLabel.Text = "Welcome, guest!";
            await RootPage.NavigateFromMenu(0);

        }
        protected override void OnAppearing()
        {

            if (isLoggedIn == "no")
            {
                LogoutButton.IsVisible = false;
                LoginButton.IsVisible = true;
                UserLabel.Text = "Welcome, guest!";
            }
            else
            {
                LogoutButton.IsVisible = true;
                LoginButton.IsVisible = false;
                UserLabel.Text = "Hey, "+currentUser+"!";
            }
        }

    }
}