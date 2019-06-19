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
    public partial class RegisterPage : ContentPage
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
        public RegisterPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(100);
        }

        public async void RegisterUser(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            RegisterButton.Text = "Registering...";
            if (!string.IsNullOrEmpty(NameEntry.Text))
            {
                string userName = NameEntry.Text.Trim();
                string pass = PassEntry.Text;
                string passConfirm = PassEntryConfirm.Text;
                string email = EmailEntry.Text;

                if (String.Equals(pass, passConfirm))
                {

                    Regex r = new Regex("^[a-zA-Z0-9_]*$");

                    if (!r.IsMatch(userName))
                    {
                        await DisplayAlert("Error!", "Username or password invalid", "Dang");
                    }
                    else if (userName.Length > 32 || userName.Length < 3)
                    {
                        await DisplayAlert("Error!", "Your username must be between 3 and 32 characters long.", "Dang");
                    }
                    else if (!email.Contains("@"))
                    {
                        await DisplayAlert("Error!", "Invalid email", "Try again");
                    }
                    else
                    {
                        var values = new Dictionary<string, string>{
                       { "username", userName },
                       { "password", pass },
                       { "device_id", devicePushId },
                        { "email", email }
                    };

                        var content = new FormUrlEncodedContent(values);
                        var response = await client.PostAsync(App.HolidailyHost + "/portal/register/", content);
                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                        int status = responseJSON.StatusCode;
                        string message = responseJSON.Message;

                        if (status == 200)
                        {
                            await DisplayAlert("Confirm it", "We sent you a confirmation email!", "OK");
                            await Navigation.PopModalAsync();
                        }
                        else if (status == 1000)
                        {

                            await DisplayAlert("Error!", message, "Dang");
                        }
                        else
                        {
                            await DisplayAlert("Sorry!", "Something went wrong on our end", "Alrighty");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Wait a minute...", "Your passwords didn't match.", "Try again");
                }
            }
            RegisterButton.Text = "Register";
            this.IsEnabled = true;

        }
        async void CancelRegister(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Navigation.PopModalAsync();
            this.IsEnabled = true;
        }
    }
}