using System;
using EventApp.ViewModels;
using EventApp.Views;
using Foundation;
using Google.MobileAds;
using UIKit;
using Xamarin.Forms;
using Plugin.PushNotification;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using BadgeView.iOS;

namespace EventApp.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, INotifyPropertyChanged
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
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
        public static NavigationPage NavigationPage { get; private set; }
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            MobileAds.Configure("ca-app-pub-9382412071078825~2829867889");
            Rg.Plugins.Popup.Popup.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            LoadApplication(new App());
            PushNotificationManager.Initialize(options, true);
            CircleViewRenderer.Initialize();
            Stormlion.PhotoBrowser.iOS.Platform.Init();
            return base.FinishedLaunching(app, options);
        }
        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            PushNotificationManager.DidRegisterRemoteNotifications(deviceToken);
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            PushNotificationManager.RemoteNotificationRegistrationFailed(error);
        }
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            PushNotificationManager.DidReceiveMessage(userInfo);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {

            if (url.Query.Contains("activated"))
            {
                try
                {
                    string token = Regex.Match(url.Query, @"token=(.+)").Groups[1].ToString();
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

                var menuPage = new MenuPage(); // Build hamburger menu
                NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
                var rootPage = new RootPage(); // Root handles master detail navigation
                rootPage.Master = menuPage; // Menu
                rootPage.Detail = NavigationPage; // Content
                App.Current.MainPage = rootPage; // Set root to built master detail
            }
            else
            {
                var menuPage = new MenuPage(); // Build hamburger menu
                NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
                var rootPage = new RootPage(); // Root handles master detail navigation
                rootPage.Master = menuPage; // Menu
                rootPage.Detail = NavigationPage; // Content
                App.Current.MainPage = rootPage; // Set root to built master detail
                try
                {
                    string holidayId = url.Query.Split("=")[1];
                    NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
                }
                catch
                {
                }

            }
            return true;
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
