using EventApp.Models;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }

        List<HomeMenuItem> menuItems;

        public string isLoggedIn
        {
            get { return Settings.GeneralSettings; }
            set
            {
                if (Settings.GeneralSettings == value)
                    return;
                Settings.GeneralSettings = value;
                OnPropertyChanged();
            }
        }

        public MenuPage()
        {
            InitializeComponent();
            menuItems = new List<HomeMenuItem>
            {
                new HomeMenuItem {Id = MenuItemType.Browse, Title="Today", Image=new Image {Source="EventApp.iOS.Resources.user_menu.png"}}
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

        public void LogoutUser(object sender, EventArgs e)
        {
            isLoggedIn = "no";
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}