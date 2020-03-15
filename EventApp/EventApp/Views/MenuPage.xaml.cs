using EventApp.Models;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        RootPage RootPage { get => Application.Current.MainPage as RootPage; }

        List<HomeMenuItem> menuItems;

        public bool isLoggedIn
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

        public string confettiCount
        {
            get { return Settings.ConfettiCount; }
            set
            {
                if (Settings.ConfettiCount == value)
                    return;
                Settings.ConfettiCount = value;
                OnPropertyChanged();
            }
        }

        public string appInfo
        {
            get { return Settings.AppInfo; }
            set
            {
                if (Settings.AppInfo == value)
                    return;
                Settings.AppInfo = value;
                OnPropertyChanged();
            }
        }

        public bool isPremium
        {
            get { return Settings.IsPremium; }
            set
            {
                if (Settings.IsPremium == value)
                    return;
                Settings.IsPremium = value;
                OnPropertyChanged();
            }
        }

        public NavigationPage NavigationPage { get; private set; }
        public MenuPage()
        {
            InitializeComponent();

            menuItems = new List<HomeMenuItem>
            {
                new HomeMenuItem {Id = MenuItemType.Holidays, Title="Home", MenuImage="today_icon.png"},
                new HomeMenuItem {Id = MenuItemType.Search, Title="Search", MenuImage="search.png"},
                new HomeMenuItem {Id = MenuItemType.AddHoliday, Title="Submit Holiday", MenuImage="pencil.png"},
                new HomeMenuItem {Id = MenuItemType.ConfettiLeaders, Title="Confetti Leaders", MenuImage="party_popper_icon.png"},
                new HomeMenuItem {Id = MenuItemType.Trending, Title="Popular", MenuImage="trending.png"},
                new HomeMenuItem {Id = MenuItemType.Updates, Title="News", MenuImage="news.png"},
                new HomeMenuItem {Id = MenuItemType.Premium, Title="Premium", MenuImage="premium.png"},
                new HomeMenuItem {Id = MenuItemType.Rewards, Title="Rewards", MenuImage="trophy.png"}
            };

            ListViewMenu.ItemsSource = menuItems;
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                var id = (int)((HomeMenuItem)e.SelectedItem).Id;
                await RootPage.NavigateFromMenu(id);
            }; 

        }

        public async void PromptLogin(object sender, EventArgs e) {
            LoginButton.IsEnabled = false;
            await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            LoginButton.IsEnabled = true;
        }

        public void LogoutUser(object sender, EventArgs e)
        {

            isLoggedIn = false;
            LogoutButton.IsVisible = false;
            LoginButton.IsVisible = true;
            DefaultHeader.IsVisible = true;
            ProfileHeader.IsVisible = false;
            HeaderDivider.IsVisible = false;
            UserLabel.Text = "Hey there!";
            currentUser = null;
            isPremium = false;

            var menuPage = new MenuPage(); // Build hamburger menu
            NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
            var rootPage = new RootPage(); // Root handles master detail navigation
            rootPage.Master = menuPage; // Menu
            rootPage.Detail = NavigationPage; // Content
            Application.Current.MainPage = rootPage; // Set root to built master detail

        }
        protected override async void OnAppearing()
        {
            AppInfoLabel.Text = appInfo;
            if(isLoggedIn)
            {
                DefaultHeader.IsVisible = false;
                ProfileHeader.IsVisible = true;
                LogoutButton.IsVisible = true;
                LoginButton.IsVisible = false;
                HeaderDivider.IsVisible = true;
                UserLabel.Text = "Hey, " + currentUser + "!";
                UserNameHeader.Text = currentUser;
                var values = new Dictionary<string, string>{
                   { "username", currentUser }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await App.globalClient.PostAsync(App.HolidailyHost + "/users/", content);
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                dynamic results = responseJSON.results;
                var confettiCount = results.confetti;
                UserPointsHeader.Text = confettiCount;
                if (isPremium)
                {
                    isPremiumLabel.Text = "Premium";
                }

            }
            else
            {
                DefaultHeader.IsVisible = true;
                ProfileHeader.IsVisible = false;
                LogoutButton.IsVisible = false;
                LoginButton.IsVisible = true;
                HeaderDivider.IsVisible = false;
                UserLabel.Text = "Hey there!";
            }

        }

    }
}