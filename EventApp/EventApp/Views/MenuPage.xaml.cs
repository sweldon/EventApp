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

        public NavigationPage NavigationPage { get; private set; }
        public MenuPage()
        {
            InitializeComponent();

            menuItems = new List<HomeMenuItem>
            {
                new HomeMenuItem {Id = MenuItemType.Browse, Title="Today", Image=new Image {Source="user_menu.png"}},
                new HomeMenuItem {Id = MenuItemType.Trending, Title="Trending", Image=new Image {Source="user_menu.png"}}
            };

            ListViewMenu.ItemsSource = menuItems;

            ListViewMenu.SelectedItem = menuItems[0];
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                var id = (int)((HomeMenuItem)e.SelectedItem).Id;
                await RootPage.NavigateFromMenu(id);
            };

        }

        public async Task PromptLogin(object sender, EventArgs e) {
            LoginButton.IsEnabled = false;
            await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            LoginButton.IsEnabled = true;
        }

        public async Task LogoutUser(object sender, EventArgs e)
        {

            isLoggedIn = "no";
            LogoutButton.IsVisible = false;
            LoginButton.IsVisible = true;
            await RootPage.NavigateFromMenu(1);

        }
        protected override void OnAppearing()
        {

            if (isLoggedIn == "no")
            {
                LogoutButton.IsVisible = false;
                LoginButton.IsVisible = true;
            }
            else
            {
                LogoutButton.IsVisible = true;
                LoginButton.IsVisible = false;
            }
        }

    }
}