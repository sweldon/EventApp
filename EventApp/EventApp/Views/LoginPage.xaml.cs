using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AppCenter;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
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

        public NavigationPage NavigationPage { get; private set; }
        public LoginPage()
        {
            InitializeComponent();
        }

        public async void Recover(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Task.Run(() => Xamarin.Forms.Device.OpenUri(new Uri(App.HolidailyHost + "/portal/recover")));
            this.IsEnabled = true;
        }
            public async void LoginUser(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            LoginButton.Text = "Logging in...";
            if (!string.IsNullOrEmpty(NameEntry.Text))
            {
                string userName = NameEntry.Text.Trim();
                string pass = PassEntry.Text;
                Debug.WriteLine(userName);
                var values = new Dictionary<string, string>{
                    { "username", userName },
                    { "password", pass },
                    { "device_id", devicePushId }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await App.globalClient.PostAsync(App.HolidailyHost + "/accounts/login/", content);
                Debug.WriteLine(response);
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

                int status = responseJSON.status;

                if (status == 200)
                {
                    isLoggedIn = true;
                    var menuPage = new MenuPage();
                    NavigationPage = new NavigationPage(new HolidaysPage());
                    var rootPage = new RootPage(); 
                    rootPage.Master = menuPage; 
                    rootPage.Detail = NavigationPage;
                    currentUser = userName;
                    isPremium = responseJSON.results.is_premium;

                    // Set some useful global user properties (maybe use this for username
                    // ETC... instead of Settings plugin? Possible improvement.
                    App.GlobalUserObject.Confetti = responseJSON.results.confetti;
                    await Navigation.PopModalAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Could not log you in. Please try again", "Okay");
                }
            }
            else
            {
                await DisplayAlert("Error", "Enter some text first!", "Alrighty");
            }
            LoginButton.Text = "Login";
            this.IsEnabled = true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(100);
        }

        public async void RegisterUser(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Navigation.PushModalAsync(new NavigationPage(new RegisterPage()));
            this.IsEnabled = true;
        }
        async void CancelLogin(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Navigation.PopModalAsync();
            this.IsEnabled = true;
        }
    }
}