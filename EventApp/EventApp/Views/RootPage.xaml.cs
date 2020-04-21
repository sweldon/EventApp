using EventApp.Models;
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

        async void OpenNotifications(object sender, EventArgs e)
        {
            BellBtn.IsEnabled = false;
            await Navigation.PushModalAsync(new NavigationPage(new NotificationsPage()));
            await Task.Delay(2000);
            BellBtn.IsEnabled = true;
        }

        public async Task NavigateFromMenu(int id)
        {
            if (!MenuPages.ContainsKey(id))
            {
                switch (id)
                {
                    case (int)MenuItemType.Notifications:
                        MenuPages.Add(id, new NavigationPage(new NotificationsPage()));
                        break;
                    case (int)MenuItemType.Holidays:
                        MenuPages.Add(id, new NavigationPage(new HolidaysPage()));
                        break;
                    case (int)MenuItemType.Search:
                        MenuPages.Add(id, new NavigationPage(new SearchPage()));
                        break;
                    case (int)MenuItemType.Premium:
                        MenuPages.Add(id, new NavigationPage(new Premium()));
                        break;
                    case (int)MenuItemType.Rewards:
                        MenuPages.Add(id, new NavigationPage(new RewardsPage()));
                        break;
                    case (int)MenuItemType.AddHoliday:
                        MenuPages.Add(id, new NavigationPage(new AddHoliday()));
                        break;
                    case (int)MenuItemType.ConfettiLeaders:
                        MenuPages.Add(id, new NavigationPage(new ConfettiLeaders()));
                        break;
                    case (int)MenuItemType.About:
                        MenuPages.Add(id, new NavigationPage(new About()));
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