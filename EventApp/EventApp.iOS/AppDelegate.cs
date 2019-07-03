using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EventApp.ViewModels;
using EventApp.Views;
using Foundation;
using Google.MobileAds;
using UIKit;
using Xamarin.Forms;

namespace EventApp.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public NavigationPage NavigationPage { get; private set; }
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            MobileAds.Configure("ca-app-pub-9382412071078825~2829867889");
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            string holidayId = url.Query.Split("=")[1];
            var menuPage = new MenuPage(); // Build hamburger menu
            NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
            var rootPage = new RootPage(); // Root handles master detail navigation
            rootPage.Master = menuPage; // Menu
            rootPage.Detail = NavigationPage; // Content
            App.Current.MainPage = rootPage; // Set root to built master detail
            //System.Diagnostics.Debug.WriteLine("OPENING HOLIDAY " + data);
            NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));

            return true;
        }

    }


}
