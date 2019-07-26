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

        public NavigationPage NavigationPage { get; private set; }
        public MenuPage()
        {
            InitializeComponent();

            menuItems = new List<HomeMenuItem>
            {
                new HomeMenuItem {Id = MenuItemType.Holidays, Title="Home", MenuImage="today_icon.png"},
                new HomeMenuItem {Id = MenuItemType.Search, Title="Search", MenuImage="search.png"},
                new HomeMenuItem {Id = MenuItemType.AddHoliday, Title="Submit Holiday", MenuImage="pencil.png"},
                //new HomeMenuItem {Id = MenuItemType.Notifications, Title="Notifications", MenuImage="alarm.png"},
                new HomeMenuItem {Id = MenuItemType.ConfettiLeaders, Title="Confetti Leaders", MenuImage="party_popper_icon.png"},
                new HomeMenuItem {Id = MenuItemType.Trending, Title="Popular", MenuImage="trending.png"},
                new HomeMenuItem {Id = MenuItemType.Updates, Title="News", MenuImage="news.png"},
                new HomeMenuItem {Id = MenuItemType.Premium, Title="Premium", MenuImage="premium.png"},
                //new HomeMenuItem {Id = MenuItemType.Rewards, Title="Rewards", MenuImage="trophy.png"}
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
            DefaultHeader.IsVisible = true;
            ProfileHeader.IsVisible = false;
            HeaderDivider.IsVisible = false;
            UserLabel.Text = "Hey there!";
            currentUser = null;
            await RootPage.NavigateFromMenu(0);

        }
        protected override async void OnAppearing()
        {
            AppInfoLabel.Text = appInfo;
            if (isLoggedIn == "no")
            {
                DefaultHeader.IsVisible = true;
                ProfileHeader.IsVisible = false;
                LogoutButton.IsVisible = false;
                LoginButton.IsVisible = true;
                HeaderDivider.IsVisible = false;
                UserLabel.Text = "Hey there!";
            }
            else
            {
                DefaultHeader.IsVisible = false;
                ProfileHeader.IsVisible = true;
                LogoutButton.IsVisible = true;
                LoginButton.IsVisible = false;
                HeaderDivider.IsVisible = true;
                UserLabel.Text = "Hey, " + currentUser + "!";
                UserNameHeader.Text = currentUser;

                // Update points
                var values = new Dictionary<string, string>{
                { "user", currentUser }
                };

                var content = new FormUrlEncodedContent(values);
                HttpClient client = new HttpClient();
                var response = await client.PostAsync(App.HolidailyHost + "/portal/get_user_rewards/", content);
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int points = responseJSON.Points;
                UserPointsHeader.Text = points.ToString();

            }

        }

    }
}