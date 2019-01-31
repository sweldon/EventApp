using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EventApp.Views;
using System.Diagnostics;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Push;
using Microsoft.AppCenter.Analytics;
using EventApp.Models;
using EventApp.ViewModels;
using EventApp.Services;
using System;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace EventApp
{
    public partial class App : Application
    {

        public NavigationPage NavigationPage { get; private set; }
        public Holiday OpenHolidayPage { get; set; }
        public Comment OpenComment { get; set; }

        public string devicePushId
        {
            get { return Settings.DevicePushId; }
            set
            {
                if (Settings.DevicePushId == value)
                    return;
                Settings.DevicePushId = value;
                OnPropertyChanged();
            }
        }

        public bool isActive
        {
            get { return Settings.IsActive; }
            set
            {
                if (Settings.IsActive == value)
                    return;
                Settings.IsActive = value;
                OnPropertyChanged();
            }
        }

        public App()
        {
            InitializeComponent();

            var menuPage = new MenuPage(); // Build hamburger menu
            NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
            var rootPage = new RootPage(); // Root handles master detail navigation
            rootPage.Master = menuPage; // Menu
            rootPage.Detail = NavigationPage; // Content
            MainPage = rootPage; // Set root to built master detail

        }

        protected override void OnStart()
        {
       
            Debug.WriteLine("User initial active value: " + isActive);
            if (!AppCenter.Configured)
            {
                Push.PushNotificationReceived += (sender, e) =>
                {
                    if (e.Message == null)
                    {
                        // Background Android
                        if (e.CustomData.ContainsKey("comment_id"))
                        {
                            string commentId = e.CustomData["comment_id"];
                            string holidayId = e.CustomData["holiday_id"];
                            NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
                            NavigationPage.PushAsync(new CommentPage(new CommentViewModel(commentId, holidayId)));
                        }
                        else if (e.CustomData.ContainsKey("holiday_id"))
                        {
                            string holidayId = e.CustomData["holiday_id"];
                            NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
                        }
                    }
                    else
                    {
                        // Foreground Android
                        if (e.CustomData.ContainsKey("comment_id")) {
                            string commentId = e.CustomData["comment_id"];
                            string commentUser = e.CustomData["comment_user"];
                            string holidayId = e.CustomData["holiday_id"];
                            AlertUser(commentId, holidayId, commentUser);
                        }
                        else if (e.CustomData.ContainsKey("holiday_id"))
                        {
                            string holidayId = e.CustomData["holiday_id"];
                            AlertUserHolidays(holidayId);
                        }
                    }
                };
            }

            AppCenter.Start("android=bcc3eb00-cbdb-4d66-82f7-860eb3b56e56;ios=296e7478-5c95-4aa1-b904-b43c78377d1c;", typeof(Push), typeof(Analytics));
            devicePushId = AppCenter.GetInstallIdAsync().Result.Value.ToString();
        }

        async void AlertUser(string commentId, string holidayId, string commentUser)
        {

            var title = commentUser + " mentioned you!";   
            var userAlert = await Application.Current.MainPage.DisplayAlert(title, "", "Go to Comment", "Close");
            if (userAlert) {
                await NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
                //OpenComment = new Comment { Id = commentId, Content = content, UserName = commentUser, TimeSince = TimeAgo };
                await NavigationPage.PushAsync(new CommentPage(new CommentViewModel(commentId, holidayId)));
            }

        }

        async void AlertUserHolidays(string holidayId)
        {

            var title = "Todays holidays are out!";
            var userAlert = await Application.Current.MainPage.DisplayAlert(title, "Want to see a random one?", "OK", "Close");
            if (userAlert)
            {
                await NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
            }
        }

        protected override void OnSleep()
        {
            isActive = false;
        }

        protected override void OnResume()
        {
            isActive = true;
        }

        public string GetRelativeTime(DateTime commentDate)
        {

            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            DateTime currentDate = DateTime.Now;
            var ts = new TimeSpan(currentDate.Ticks - commentDate.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return "Just Now";

            if (delta < 2 * MINUTE)
                return "a minute ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * MINUTE)
                return "an hour ago";

            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                return "yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " days ago";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }

        }

    }
}
