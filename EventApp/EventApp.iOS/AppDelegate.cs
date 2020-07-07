using System;
using EventApp.ViewModels;
using EventApp.Views;
using Foundation;
using Google.MobileAds;
using UIKit;
using Xamarin.Forms;
using ImageCircle.Forms.Plugin.iOS;
using Plugin.PushNotification;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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
        }
        public bool tokenUsed
        {
            get { return Settings.ActivationTokenUsed; }
            set
            {
                if (Settings.ActivationTokenUsed == value)
                    return;
                Settings.ActivationTokenUsed = value;
                OnPropertyChanged();
            }
        }
        public static NavigationPage NavigationPage { get; private set; }
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            ImageCircleRenderer.Init();
            MobileAds.Configure("ca-app-pub-9382412071078825~2829867889");
            Rg.Plugins.Popup.Popup.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            LoadApplication(new App());
            PushNotificationManager.Initialize(options, true);
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
        // To receive notifications in foregroung on iOS 9 and below.
        // To receive notifications in background in any iOS version
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
                    // TODO check if token has been used in new settings var
                    if (activationToken == token && !tokenUsed)
                    {
                        isLoggedIn = true;
                        tokenUsed = true;
                    }
                }
                catch
                {

                }

                Utils.BuildNavigation();
            }
            else
            {
                Utils.BuildNavigation();
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
