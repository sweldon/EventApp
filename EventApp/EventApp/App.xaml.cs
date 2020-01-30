using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EventApp.Views;
using System.Diagnostics;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Push;
using Microsoft.AppCenter.Analytics;
using EventApp.Models;
using EventApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace EventApp
{
    public partial class App : Application
    {

        public NavigationPage NavigationPage { get; private set; }
        public Holiday OpenHolidayPage { get; set; }
        public Comment OpenComment { get; set; }
        //public static string HolidailyHost = "https://holidailyapp.com";
        //public static string HolidailyHost = "http://10.0.2.2:8000";
        public static string HolidailyHost = "http://localhost:8888";
        public static HttpClient globalClient = new HttpClient();
        // App-wide reusable instance for choosing random ads
        public static Random randomGenerator = new Random();
        public static User GlobalUserObject = new User();

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

        protected override async void OnStart()
        {


            

            if (!AppCenter.Configured){
                Push.PushNotificationReceived += (sender, e) =>
                {
                    if (e.Message == null)
                    {
                        // Background Android
                        var menuPage = new MenuPage(); // Build hamburger menu
                        NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
                        var rootPage = new RootPage(); // Root handles master detail navigation
                        rootPage.Master = menuPage; // Menu
                        rootPage.Detail = NavigationPage; // Content
                        MainPage = rootPage; // Set root to built master detail

                        if (e.CustomData.ContainsKey("news"))
                        {
                            NavigationPage.PushAsync(new Updates());
                        }
                        else if (e.CustomData.ContainsKey("comment_id"))
                        {
                            string commentId = e.CustomData["comment_id"];
                            string holidayId = e.CustomData["holiday_id"];

                            NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId, null)));
                            NavigationPage.PushAsync(new CommentPage(new CommentViewModel(commentId, holidayId)));
                        }
                        else if (e.CustomData.ContainsKey("holiday_id"))
                        {
                            string holidayId = e.CustomData["holiday_id"];
                            NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId, null)));
                        }
                    }
                    else
                    {
                        // Foreground Android
                        if (e.CustomData.ContainsKey("news"))
                        {
                            NavigationPage.PushAsync(new Updates());
                        }
                        else if (e.CustomData.ContainsKey("comment_id"))
                        {
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


            AppCenter.Start("android=a43f5f54-cb5f-4ad2-af75-762b151c5891;ios=8fbf5b9b-1791-4ec5-bc9d-dead805d66a8;", typeof(Push), typeof(Analytics));
            devicePushId = AppCenter.GetInstallIdAsync().Result.Value.ToString();


            if (isLoggedIn)
            {
                try
                {
                    var values = new Dictionary<string, string>{
                        { "username", currentUser },
                    };

                    var content = new FormUrlEncodedContent(values);
                    var response = await App.globalClient.PostAsync(App.HolidailyHost + "/users/", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                    bool active = responseJSON.results.is_active;
                    bool isPremium = responseJSON.results.is_premium;
                    GlobalUserObject.UserName = currentUser;
                    GlobalUserObject.Confetti = confettiCount;
                    if (!active)
                    {
                        App.Current.MainPage = new NavigationPage(new LimboPage());
                    }

                }
                catch
                {
                    isPremium = false;
                }


            }
            else
            {
                isPremium = false;
            }

        }

        async void AlertUser(string commentId, string holidayId, string commentUser)
        {

            var title = commentUser + " mentioned you!";   
            var userAlert = await Application.Current.MainPage.DisplayAlert(title, "", "Go to Comment", "Close");
            if (userAlert) {
                var menuPage = new MenuPage(); // Build hamburger menu
                NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
                var rootPage = new RootPage(); // Root handles master detail navigation
                rootPage.Master = menuPage; // Menu
                rootPage.Detail = NavigationPage; // Content
                MainPage = rootPage; // Set root to built master detail
                await NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId, null)));
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
                await NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId, null)));
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
