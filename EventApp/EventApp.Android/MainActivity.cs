using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;

//using System;

//using Android.App;
//using Android.Content.PM;
//using Android.OS;
////using Android.Gms.Ads;
//using System.Diagnostics;
using Android.Content;
using Xamarin.Forms;
using EventApp.ViewModels;
using EventApp.Views;
//using Plugin.InAppBilling;
using Plugin.CurrentActivity;
using Plugin.PushNotification;
//using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EventApp.Droid
{
    [Activity(Label = "Holidaily", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(
        new[] { Intent.ActionView },
            Categories = new[] {
                Intent.CategoryDefault, Intent.CategoryBrowsable
            },
            DataPathPrefixes = new string[] { "/portal/activate", "/holiday" },
            DataSchemes = new string[] { "http", "https" },
            DataHosts = new string[] { "10.0.2.2", "holidailyapp.com" }
        )
    ]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, INotifyPropertyChanged
    {


        public NavigationPage NavigationPage { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
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
        public string activationToken
        {
            get { return Settings.ActivationToken; }
            set
            {
                if (Settings.ActivationToken == value)
                    return;
                Settings.ActivationToken = value;
                OnPropertyChanged();
            }
        }
        public string devicePushId
        {
            get { return Settings.DevicePushId; }
        }

        public bool refreshToken
        {
            get { return Settings.RefreshToken; }
            set
            {
                if (Settings.RefreshToken == value)
                    return;
                Settings.RefreshToken = value;
                OnPropertyChanged();
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            //Set the default notification channel for your app when running Android Oreo
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                //Change for your default notification channel id here
                PushNotificationManager.DefaultNotificationChannelId = "DefaultChannel";

                //Change for your default notification channel name here
                PushNotificationManager.DefaultNotificationChannelName = "General";
            }


            #if DEBUG
                PushNotificationManager.Initialize(this, true);
            #else
                PushNotificationManager.Initialize(this, refreshToken);
            #endif

            refreshToken = false;

            Rg.Plugins.Popup.Popup.Init(this);



            


            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            //MobileAds.Initialize(ApplicationContext, "ca-app-pub-9382412071078825~2735085847");

            // In-app purchase
            //CrossCurrentActivity.Current.Init(this, savedInstanceState);

            // FFImageLoading
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);

            //Stormlion.PhotoBrowser.Droid.Platform.Init(this);

            var incomingLink = Intent?.Data?.ToString();
            var holidayId = Intent?.Data?.GetQueryParameter("id");
            if (!string.IsNullOrEmpty(holidayId))
            {


                Device.BeginInvokeOnMainThread(() =>
                {
                    var menuPage = new MenuPage(); // Build hamburger menu
                    NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
                    var rootPage = new RootPage(); // Root handles master detail navigation
                    rootPage.Master = menuPage; // Menu
                    rootPage.Detail = NavigationPage; // Content
                    App.Current.MainPage = rootPage; // Set root to built master detail
                    NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId, null)));
                });


            }
            else if (!string.IsNullOrEmpty(incomingLink) && incomingLink.Contains("rewards"))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    var menuPage = new MenuPage(); // Build hamburger menu
                    NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
                    var rootPage = new RootPage(); // Root handles master detail navigation
                    rootPage.Master = menuPage; // Menu
                    rootPage.Detail = NavigationPage; // Content
                    App.Current.MainPage = rootPage; // Set root to built master detail
                    NavigationPage.PushAsync(new RewardsPage());
                });
            }
            else if (!string.IsNullOrEmpty(incomingLink) && incomingLink.Contains("activate"))
            {
                string[] linkContents = incomingLink.Split("/");
                // Trailing slash, -2 index
                string token = linkContents[linkContents.Length - 2];
                try
                {
                    if (activationToken == token &&
                        !string.IsNullOrEmpty(activationToken))
                    {
                        isLoggedIn = true;
                        activationToken = "";
                    }
                }
                catch
                {

                }
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



            LoadApplication(new App());

            PushNotificationManager.ProcessIntent(this, Intent);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            //InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
        }

        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
            }
            else
            {
                // Do something if there are not any pages in the `PopupStack`
            }
        }

    }
}
