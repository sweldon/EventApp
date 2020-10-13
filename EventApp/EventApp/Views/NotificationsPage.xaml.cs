using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventApp.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EventApp.Models;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Net.Http;
using Newtonsoft.Json;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationsPage : ContentPage
    {
        Comment comment;

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
        public bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
        }
        public string currentUser
        {
            get { return Settings.CurrentUser; }
        }

        public int notifCount
        {
            get { return Settings.NotificationCount; }
            set
            {
                if (Settings.NotificationCount == value)
                    return;
                Settings.NotificationCount = value;
                OnPropertyChanged();
            }
        }

        public bool NotificationLock;
        private ObservableCollection<Notification> notifications;
        public NotificationsPage()
        {
            InitializeComponent();
            Title = "Notifications";
            NotificationsList.ItemSelected += OnItemSelected;
            NotificationLock = false;
            notifications = new ObservableCollection<Notification>();
        }

        async void GoBack(object sender, EventArgs e)
        {
            BackBtn.IsEnabled = false;
            await Navigation.PopModalAsync();
            await Task.Delay(2000);
            BackBtn.IsEnabled = true;
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            var notif = args.SelectedItem as Notification;

            if (!NotificationLock)
            {
                NotificationLock = true;
                try
                {
                    Holiday holiday = await ApiHelpers.GetHolidayById(notif.HolidayId);
                    await Navigation.PushAsync(new HolidayDetailPage(
                        new HolidayDetailViewModel(notif.HolidayId, holiday)));
                }
                catch
                {
                    await DisplayAlert("Uh oh!", "We could no longer find that " +
                        "notification", "OK");
                }
                await Task.Delay(1000);
                NotificationLock = false;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (notifications.Count == 0 && isLoggedIn)
            {

                notifications = await GetNotifications();
                NotificationsList.ItemsSource = notifications;

                if (notifications.Count == 0)
                {
                    NoResults.IsVisible = true;
                }
                else
                {
                    NoResults.IsVisible = false;
                }
                //AdBanner.IsVisible = !isPremium;
            }
            else
            {
                if(!isLoggedIn)
                    NoResults.IsVisible = true;
            }
            
        }

        protected async void RefreshNotifications(object sender, EventArgs e)
        {
            notifications.Clear();
            notifications = await GetNotifications();
            NotificationsList.ItemsSource = notifications;
            NotificationsList.EndRefresh();
        }

        private async Task<ObservableCollection<Notification>> GetNotifications()
        {

            try
            {
                var values = new Dictionary<string, string>{
                    { "username", currentUser },
                    { "clear_notifications", "true" },
                };

                var content = new FormUrlEncodedContent(values);

                var response = await App.globalClient.PostAsync(App.HolidailyHost + "/notifications/", content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                dynamic notifList = responseJSON.results;
                foreach (var n in notifList)
                {
                    Color bg_color = n.read == true ? Color.FromHex("FFFFFF") : Color.FromHex("ebf3fd");
                    notifications.Add(new Notification()
                    {
                        Id = n.notification_id,
                        Type = n.notification_type,
                        Title = n.title,
                        Read = n.read,
                        Content = n.content,
                        TimeSince = n.time_since,
                        Icon = n.icon == null ? "icon_splash.png" : n.icon,
                        BackgroundColor = bg_color,
                        HolidayId = n.holiday_id
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return await Task.FromResult(notifications);
        }

    }
}