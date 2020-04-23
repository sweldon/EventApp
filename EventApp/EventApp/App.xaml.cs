using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EventApp.Views;
using System.Diagnostics;
using EventApp.Models;
using EventApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using Plugin.PushNotification;
using Badge.Plugin;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace EventApp
{
    public partial class App : Application
    {

        public NavigationPage NavigationPage { get; private set; }
        public Holiday OpenHolidayPage { get; set; }
        public Comment OpenComment { get; set; }

        #if DEBUG
            #if __IOS__
                    public static string HolidailyHost = "http://localhost:8888";
            #else
                    public static string HolidailyHost = "http://10.0.2.2:8000";
            #endif
        #else
            public static string HolidailyHost = "https://holidailyapp.com";
        #endif

        public static HttpClient globalClient = new HttpClient();
        // App-wide reusable instance for choosing random ads
        public static Random randomGenerator = new Random();


        public static async void popModalIfActive(INavigation nav)
        {
            #if __IOS__
                var isModalShowing = Application.Current.MainPage?.Navigation?.ModalStack?.LastOrDefault();
                if (isModalShowing != null)
                {
                    Debug.WriteLine($"Modal closed using swipe, popping before re-opening");
                    await nav.PopModalAsync();
                }
            #endif
        }

        public static async void promptLogin(INavigation nav)
        {
            popModalIfActive(nav);
            await nav.PushModalAsync(new NavigationPage(new LoginPage()));
        }

        public bool eulaAccepted
        {
            get { return Settings.EulaAccepted; }
            set
            {
                if (Settings.EulaAccepted == value)
                    return;
                Settings.EulaAccepted = value;
                OnPropertyChanged();
            }
        }

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
        public bool OpenNotifications
        {
            get { return Settings.OpenNotifications; }
            set
            {
                if (Settings.OpenNotifications == value)
                    return;
                Settings.OpenNotifications = value;
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
        private async void MakeUserActive()
        {
            await Task.Delay(5000);
            isActive = true;
        }
        private Dictionary<string, string>
            ConsumePush(IDictionary<string, object> pushData)
        {

            string pushType = null;
            string holidayId = null;
            string commentId = null;
            string commentUser = null;
            foreach (var data in pushData)
            {
                if (data.Key == "push_type")
                {
                    pushType = data.Value.ToString();
                }
                else if (data.Key == "comment_id")
                {
                    commentId = data.Value.ToString();
                }
                else if (data.Key == "holiday_id")
                {
                    holidayId = data.Value.ToString();
                }
                else if (data.Key == "comment_user")
                {
                    commentUser = data.Value.ToString();
                }
            }
            var parsedData = new Dictionary<string, string> {
                {"push_type", pushType },
                {"holiday_id", holidayId },
                {"comment_id", commentId },
                {"comment_user", commentUser },
            };
            return parsedData;
        }
        protected override async void OnStart()
        {
            await Task.Run(async () =>
            {
                MakeUserActive();
            });
            // Clear badge notifications
            CrossBadge.Current.ClearBadge();
            CrossPushNotification.Current.OnTokenRefresh += (s, p) =>
            {
                devicePushId = p.Token.ToString();
            };
            CrossPushNotification.Current.OnNotificationReceived += (s, p) =>
            {
                // User is active in the app
                Dictionary<string, string> pushData = ConsumePush(p.Data);
                string pushType = pushData["push_type"];
                string holidayId = pushData["holiday_id"];
                string commentId = pushData["comment_id"];
                string commentUser = pushData["comment_user"];
                if (pushType == "news")
                {
                    //AlertNews();
                }
                else if (pushType == "holiday")
                {
                    AlertUserHolidays(holidayId);
                }
                else if (pushType == "comment")
                {
                    AlertUser(commentId, holidayId, commentUser);
                }
            };
            CrossPushNotification.Current.OnNotificationOpened += (s, p) =>
            {
                // User is not in the app
                Dictionary<string, string> pushData = ConsumePush(p.Data);
                string pushType = pushData["push_type"];
                string holidayId = pushData["holiday_id"];
                string commentId = pushData["comment_id"];
                string commentUser = pushData["comment_user"];
                if (pushType == "news")
                {
                    OpenNotifications = true;
                }
                else if (pushType == "holiday")
                {
                    NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
                }
                else if (pushType == "comment")
                {
                    // Only need comment_user for active forground alert
                    NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId, null, commentId)));
                }
            };
            CrossPushNotification.Current.OnNotificationDeleted += (s, p) =>
            {
                // Clear badge notifications
            };

            if (isLoggedIn)
            {
                try
                {
                    
                    var values = new Dictionary<string, string>{
                        { "username", currentUser },
                        { "device_id", devicePushId },
                        { "version", appInfo }
                    };

                    #if __IOS__
                        values["platform"] = "ios";
                    #elif __ANDROID__
                        values["platform"] = "android";
                    #endif

                    var content = new FormUrlEncodedContent(values);
                    var response = await App.globalClient.PostAsync(App.HolidailyHost + "/users/", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                    bool active = responseJSON.results.is_active;
                    isPremium = responseJSON.results.is_premium;
                    confettiCount = responseJSON.results.confetti;
                    if (!active)
                    {
                        App.Current.MainPage = new NavigationPage(new LimboPage());
                    }

                }
                catch
                {
                    // Reset labels and global settings
                    isLoggedIn = false;
                    currentUser = null;
                    isPremium = false;
                }
            }
            else
            {
                isPremium = false;
            }

            // EULA
            if (!eulaAccepted)
            {
                await Application.Current.MainPage.DisplayAlert("Welcome!",
                    "Welcome to Holidaily! You will only see this message once. " +
                    "Please view our End User License Agreement before proceeding.",
                    "View");
                App.Current.MainPage = new NavigationPage(new Eula());
            }
        }

        async void AlertUser(string commentId, string holidayId, string commentUser)
        {
            var title = commentUser + " mentioned you!";   
            var userAlert = await Application.Current.MainPage.DisplayAlert(title, "", "Go to Comment", "Close");
            if (userAlert) {
                await NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId, null, commentId)));
            }
        }

        async void AlertNews()
        {
            var title = "Holidaily just posted an announcement";
            var newsAlert = await Application.Current.MainPage.DisplayAlert(title, "", "Read Now", "Close");
            if (newsAlert)
            {
                // Modal Async requires you to be in a content page, so use settings variable
                OpenNotifications = true;
            }
        }

        async void AlertUserHolidays(string holidayId)
        {

            var title = "Todays holidays are live!";
            var userAlert = await Application.Current.MainPage.DisplayAlert(title, "Want to see today's featured holiday?", "OK", "Close");
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

    }
}
