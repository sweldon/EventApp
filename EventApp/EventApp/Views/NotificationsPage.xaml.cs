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

        public string currentUser
        {
            get { return Settings.CurrentUser; }
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

            if (notif.Type == "Comment" && !NotificationLock)
            {
                NotificationLock = true;
                try
                {
                    comment = await ApiHelpers.GetCommentById(notif.Id);
                    Holiday holiday = await ApiHelpers.GetHolidayById(comment.HolidayId);
                    await Navigation.PushAsync(new HolidayDetailPage(
                        new HolidayDetailViewModel(comment.HolidayId, holiday, notif.Id)));
                }
                catch
                {
                    await DisplayAlert("Uh oh!", "We could no longer find that comment.", "Fine");
                }
                await Task.Delay(2000);
                NotificationLock = false;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();


            if(notifications.Count == 0)
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
            }
            AdBanner.IsVisible = !isPremium;
        }

        private async Task<ObservableCollection<Notification>> GetNotifications()
        {

            var values = new Dictionary<string, string>{
                    { "username", currentUser },
                };

            var content = new FormUrlEncodedContent(values);

            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/notifications/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic notifList = responseJSON.results;
            foreach (var n in notifList)
            {
                notifications.Add(new Notification()
                {
                    Id = n.notification_id,
                    Type = n.notification_type,
                    Title = n.title,
                    Read = n.read,
                    Content = n.content,
                    TimeSince = n.time_since,
                    Icon = n.icon == null ? "icon_splash.png" : n.icon
                });
            }
            return await Task.FromResult(notifications);
        }

    }
}