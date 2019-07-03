using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Ads;
using System.Diagnostics;
using Android.Content;
using Xamarin.Forms;
using EventApp.ViewModels;
using EventApp.Views;
using Plugin.InAppBilling;
using Plugin.CurrentActivity;

namespace EventApp.Droid
{
    [Activity(Label = "Holidaily", Icon = "@drawable/Icon", Theme = "@style/splashscreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataScheme = "holidaily", DataHost = "holiday")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public NavigationPage NavigationPage { get; private set; }
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.SetTheme(Resource.Style.MainTheme);
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);      
            MobileAds.Initialize(ApplicationContext, "ca-app-pub-9382412071078825~2735085847");

            // In-app purchase
            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            var data = Intent?.Data?.GetQueryParameter("id");
            if (!string.IsNullOrEmpty(data))
            {

                Device.BeginInvokeOnMainThread(() => {
                    var menuPage = new MenuPage(); // Build hamburger menu
                    NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
                    var rootPage = new RootPage(); // Root handles master detail navigation
                    rootPage.Master = menuPage; // Menu
                    rootPage.Detail = NavigationPage; // Content
                    App.Current.MainPage = rootPage; // Set root to built master detail
                    NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(data)));
                });

            }

            LoadApplication(new App());

    }
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
    }

    }
}