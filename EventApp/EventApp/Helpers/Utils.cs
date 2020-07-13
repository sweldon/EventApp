using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EventApp.Views;
using MarcTron.Plugin.Controls;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace EventApp
{
    public class Utils
    {
        public static string currentUser
        {
            get { return Settings.CurrentUser; }
        }

        public static string devicePushId
        {
            get { return Settings.DevicePushId; }
        }

        public static bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
        }
        public static NavigationPage NavigationPage { get; private set; }
        public static string GetCelebrationImage(bool is_celebrating)
        {
            if (is_celebrating)
            {
                return "celebrate_active.png";
            }
            else
            {
                return "celebrate.png";
            }
        }
        public static string GetUpVoteImage(string vote)
        {
            // TODO: consider turning this into a global static app variable
            string upvote_active = "up_active.png";
            string upvote_neutral = "up.png";
            if (vote == "up")
            {
                return upvote_active;
            }
            else
            {
                return upvote_neutral;
            }
        }
        public static string GetDownVoteImage(string vote)
        {
            // TODO: consider turning this into a global static app variable
            string downvote_active = "down_active.png";
            string downvote_neutral = "down.png";
            if (vote == "down")
            {
                return downvote_active;
            }
            else
            {
                return downvote_neutral;
            }
        }
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public static void BuildNavigation()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var menuPage = new MenuPage(); // Build hamburger menu
                NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
                var rootPage = new RootPage(); // Root handles master detail navigation
                rootPage.Master = menuPage; // Menu
                rootPage.Detail = NavigationPage; // Content
                App.Current.MainPage = rootPage; // Set root to built master detail
            });

        }
        public static void ResetGlobalUser(string username=null)
        {
            App.GlobalUser.UserName = username != null ? username : "none";
            App.GlobalUser.Confetti = "0";
            App.GlobalUser.Comments = "0";
            App.GlobalUser.Approved = "0";
            App.GlobalUser.LastOnline = "just now";
            App.GlobalUser.Avatar = null;
            App.GlobalUser.Premium = false;
        }
        public async static void syncUser()
        {
            // Keep logged-in user devices up-to-date
            if (isLoggedIn)
            {
                try
                {
                    var values = new Dictionary<string, string>{
                        { "username", currentUser },
                        { "device_update", devicePushId },
                    };

                    // Needs platform for device type
                    #if __IOS__
                    values["platform"] = "ios";
                    #elif __ANDROID__
                    values["platform"] = "android";
                    #endif
                    var content = new FormUrlEncodedContent(values);
                    var response = await App.globalClient.PostAsync(App.HolidailyHost + "/users/", content);
                }
                catch
                {
                }
            }
            else
            {
                // Keep anonymous devices up-to-date
                try
                {
                    var values = new Dictionary<string, string>{
                        { "device_id", devicePushId },
                    };

                    // Needs platform for device type
                    #if __IOS__
                    values["platform"] = "ios";
                    #elif __ANDROID__
                    values["platform"] = "android";
                    #endif

                    var content = new FormUrlEncodedContent(values);
                    await App.globalClient.PostAsync(App.HolidailyHost + "/users/", content);
                }
                catch
                {

                }
            }
        }
        public static string GetDay()
        {
            DateTime currentDate = DateTime.Today;

            string dateString = currentDate.ToString("dd-MM-yyyy");
            string dayNumber = dateString.Split('-')[0].TrimStart('0');
            int monthNumber = Int32.Parse(dateString.Split('-')[1]);

            List<string> months = new List<string>() {
                "January","February","March","April","May","June","July",
                "August", "September", "October", "November", "December"
            };

            string monthString = months[monthNumber - 1];
            string todayString = currentDate.DayOfWeek.ToString();
            return $"{monthString} {dayNumber}";
        }
    }
}
