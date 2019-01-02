using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EventApp.Views;
using System.Diagnostics;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace EventApp
{
    public partial class App : Application
    {
    
        public string isLoggedIn
        {
            get { return Settings.GeneralSettings; }
            set
            {
                if (Settings.GeneralSettings == value)
                return;
                Settings.GeneralSettings = value;
                OnPropertyChanged();
            }
        }

        public NavigationPage NavigationPage { get; private set; }
        public App()
        {
            InitializeComponent();

            if (isLoggedIn == "no")
            {
                MainPage = new LoginPage();
            }
            else
            {
                isLoggedIn = "yes";
                var menuPage = new MenuPage(); // Build hamburger menu
                NavigationPage = new NavigationPage(new ItemsPage()); // Push main logged-in page on top of stack
                var rootPage = new MainPage(); // Root handles master detail navigation
                rootPage.Master = menuPage; // Menu
                rootPage.Detail = NavigationPage; // Content
                MainPage = rootPage; // Set root to built master detail
            }
        }

        protected override void OnStart()
        {
            // Initial startup
        }

        protected override void OnSleep()
        {
            // Sleep
        }

        protected override void OnResume()
        {
            // On resume
        }
    }
}
