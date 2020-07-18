using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EventApp.Models;

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

        public string appInfo
        {
            get { return Settings.AppInfo; }
            set
            {
                if (Settings.AppInfo == value)
                    return;
                Settings.AppInfo = value;
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

        public string confettiCount
        {
            get { return Settings.ConfettiCount; }
            set
            {
                if (Settings.ConfettiCount == value)
                    return;
                Settings.ConfettiCount = value;
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
                Device.BeginInvokeOnMainThread(() => {
                    Device.OpenUri(new Uri(App.HolidailyHost + "/portal/recover"));
                });
            this.IsEnabled = true;
        }

        public async void LoginUser(object sender, EventArgs e)
        {
            try
            {
                this.IsEnabled = false;
                LoginButton.Text = "Logging in...";
                if (!string.IsNullOrEmpty(NameEntry.Text))
                {
                    string userName = NameEntry.Text.Trim();
                    string pass = PassEntry.Text;

                    var values = new Dictionary<string, string>{
                        { "username", userName },
                        { "password", pass },
                        { "device_id", devicePushId },
                        { "version", appInfo }
                    };

                    #if __IOS__
                        values["platform"] = "ios";
                    #elif __ANDROID__
                        values["platform"] = "android";
                    #endif

                    var content = new FormUrlEncodedContent(values);
                    var response = await App.globalClient.PostAsync(App.HolidailyHost + "/accounts/login/", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                    int status = responseJSON.status;

                    if (status == 200)
                    {
                        // Set some useful global user properties (maybe use this for username
                        // ETC... instead of Settings plugin? Possible improvement.
                        //App.GlobalUserObject.Confetti = responseJSON.results.confetti;
                        isLoggedIn = true;
                        currentUser = responseJSON.results.username;
                        isPremium = responseJSON.results.premium;
                        confettiCount = responseJSON.results.confetti;
                        string avatar = responseJSON.results.profile_image;
                        App.GlobalUser.UserName = currentUser;
                        App.GlobalUser.Confetti = confettiCount;
                        App.GlobalUser.Avatar = avatar;
                        App.GlobalUser.Premium = isPremium;
                        App.GlobalUser.Comments = responseJSON.results.num_comments;
                        App.GlobalUser.Approved = responseJSON.results.approved_holidays;
                        App.GlobalUser.LastOnline = responseJSON.results.last_online;
                        MessagingCenter.Send(this, "UpdateMenu", true);
                        MessagingCenter.Send(this, "UpdateComments");
                        MessagingCenter.Send(this, "UpdateHoliday");
                        MessagingCenter.Send(this, "UpdateHolidayFeed");
                        if(avatar != null)
                            MessagingCenter.Send(this, "UpdateMenuProfilePicture", avatar);
                        try
                        {
                            await Navigation.PopModalAsync();
                        }
                        catch
                        {
                            // Don't have to pop modal, not logging in "in-line"
                            await Navigation.PopAsync();
                        }

                        // Update bell
                        await Utils.GetUserNotificationCount();

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
            catch
            {
                await DisplayAlert("Error", "Couldn't connect to Holidaily", "OK");
                LoginButton.Text = "Login";
                this.IsEnabled = true;
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