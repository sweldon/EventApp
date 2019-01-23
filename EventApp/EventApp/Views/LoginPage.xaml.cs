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

        public string devicePushId = AppCenter.GetInstallIdAsync().Result.Value.ToString();
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

        public string currentUser
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

        HttpClient client = new HttpClient();
        string ec2Instance = "http://ec2-54-156-187-51.compute-1.amazonaws.com";

        public NavigationPage NavigationPage { get; private set; }
        public LoginPage()
        {
            InitializeComponent();
        }

        public async Task LoginUser(object sender, EventArgs e)
        {
            string userName = NameEntry.Text.Trim();
            string pass = PassEntry.Text;

            if (!string.IsNullOrEmpty(userName))
            {
                var values = new Dictionary<string, string>{
                    { "username", userName },
                    { "password", pass },
                    { "device_id", devicePushId }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(ec2Instance + "/portal/login_mobile/", content); 
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
        public async Task RegisterUser(object sender, EventArgs e)
        {
            string userName = NameEntry.Text.Trim();
            string pass = PassEntry.Text;

            Regex r = new Regex("^[a-zA-Z0-9]*$");

            if (!r.IsMatch(userName))
            {
                await DisplayAlert("Error!", "Username or password invalid", "Dang");
            }
            else if(userName.Length > 32 || userName.Length < 3)
            {
                await DisplayAlert("Error!", "Your username must be between 3 and 32 characters long.", "Dang");
            }
            else
            {
                var values = new Dictionary<string, string>{
                   { "username", userName },
                   { "password", pass },
                   { "device_id", devicePushId }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(ec2Instance + "/portal/register/", content);
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int status = responseJSON.StatusCode;

                if (status == 200)
                {
                    isLoggedIn = "yes";
                    //await RootPage.Detail.Navigation.PushAsync(new ItemsPage());
                    var menuPage = new MenuPage();
                    NavigationPage = new NavigationPage(new HolidaysPage());
                    var rootPage = new RootPage();
                    rootPage.Master = menuPage;
                    rootPage.Detail = NavigationPage;
                    currentUser = userName;
                    Application.Current.MainPage = rootPage;
                }
                else if (status == 1000)
                {
                    await DisplayAlert("Error!", "Username already exists.", "Dang");
                }
                else
                {
                    await DisplayAlert("Sorry!", "Something went wrong on our end", "Alrighty");
                }
            }

        }
        async void CancelLogin(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Navigation.PopModalAsync();
            this.IsEnabled = true;
        }
    }
}