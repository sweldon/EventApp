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
        public string isLoggedIn
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

        HttpClient client = new HttpClient();

        public NavigationPage NavigationPage { get; private set; }
        public LoginPage()
        {
            InitializeComponent();
        }

        public async void LoginUser(object sender, EventArgs e)
        {


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
                var response = await client.PostAsync(App.HolidailyHost + "/portal/login_mobile/", content); 
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int status = responseJSON.StatusCode;
                string message = responseJSON.Message;

                if (status == 200)
                {
                    isLoggedIn = "yes";
                    var menuPage = new MenuPage();
                    NavigationPage = new NavigationPage(new HolidaysPage());
                    var rootPage = new RootPage(); 
                    rootPage.Master = menuPage; 
                    rootPage.Detail = NavigationPage;
                    currentUser = userName;
                    await Navigation.PopModalAsync();
                }
                else
                {
                    await DisplayAlert("Error", message, "Okay");
                }
            }
            else
            {
                await DisplayAlert("Error", "Enter some text first!", "Alrighty");
            }

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