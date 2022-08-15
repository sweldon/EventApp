using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
//using MarcTron.Plugin;
//using System.Net.Http;
//using Newtonsoft.Json;
//using MarcTron.Plugin.CustomEventArgs;
//using EventApp.Models;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;
#if __IOS__
using MarcTron.Plugin.CustomEventArgs;
#endif
namespace EventApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RewardsPage : ContentPage
	{
        private string WatchAdText = "Watch Ad for Confetti!";
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
        private string Hours;
        private string Minutes;
        private string Seconds;
        public RewardsPage ()
		{
			InitializeComponent ();

            // Test ad: ca-app-pub-3940256099942544/5224354917

            //CrossMTAdmob.Current.OnRewardedVideoStarted += (object sender, EventArgs e) => {
            //    ToggleAdButton(false);
            //};

            //CrossMTAdmob.Current.OnRewardedVideoAdLoaded += (object sender, EventArgs e) => {
            //    WatchAdButton.Text = "Watch Ad for Confetti";
            //};

            //CrossMTAdmob.Current.OnRewardedVideoAdClosed += (object sender, EventArgs e) => {

            //#if DEBUG
            //    CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-3940256099942544/5224354917");
            //#else
            //    #if __IOS__
            //        CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-9382412071078825/4201400125");
            //    #elif __ANDROID__
            //        CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-9382412071078825/7152256279");
            //    #endif
            //#endif

            //};

            //CrossMTAdmob.Current.OnRewarded += (object sender, MTEventArgs e) => {

            //    try
            //    {
            //        if (isLoggedIn)
            //        {
            //            ClaimReward("5");
            //        }
                    
            //    }
            //    catch(Exception ex)
            //    {
            //        Debug.WriteLine(ex);
            //    }

                
            //};


        }

        private void ToggleAdButton(bool show=true)
        {
            //if (show)
            //{
            //    WatchAdButton.IsVisible = true;
            //    WatchAdButton.BackgroundColor = Color.FromHex("4c96e8");
            //    WatchAdButton.Text = "Watch Ad for Confetti";
            //}
            //else
            //{
            //    WatchAdButton.IsVisible = false;
            //}
        }

        public async void UpdateUserPoints()
        {
            //try
            //{
            //    var values = new Dictionary<string, string>{
            //    { "username", currentUser }
            //    };

            //    var content = new FormUrlEncodedContent(values);
            //    var response = await App.globalClient.PostAsync(App.HolidailyHost + "/user/", content);
            //    var responseString = await response.Content.ReadAsStringAsync();

            //    dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            //    string points = responseJSON.results.ContainsKey("confetti") ? responseJSON.results.confetti.ToString() : "0";
            //    PointsLabel.Text =  points;
            //    bool notify = (bool)responseJSON.results.requested_confetti_alert;
            //    if (notify)
            //    {
            //        NotifySwitch.IsToggled = true;
            //    }
            //    NotifySwitch.Toggled -= NotifyToggled;
            //    NotifySwitch.Toggled += NotifyToggled;
            //    var confettiCooldown = responseJSON.results.confetti_cooldown;
                
            //    if (responseJSON.results.confetti_cooldown != null)
            //    {

            //        int h = confettiCooldown.hours;
            //        int m = confettiCooldown.minutes;
            //        int s = confettiCooldown.seconds;
            //        StartCooldownTimer(hours: h, minutes: m, seconds: s);
            //    }
            //    else
            //    {
            //        ToggleAdButton(true);
            //    }
            //}
            //catch(Exception ex)
            //{
            //    Debug.WriteLine(ex);
            //    //await DisplayAlert("Error", "Couldn't connect to Holidaily", "OK");
            //}

        }

        void ShowDoneBanner()
        {
            //CountdownWrapper.BackgroundColor = Color.Green;
            //TimerWrapper.IsVisible = false;
            //CompleteWrapper.IsVisible = true;
            //NotifyWrapper.IsVisible = false;
            //ToggleAdButton(true);
        }

        void ShowTimerBanner()
        {
            //CountdownWrapper.IsVisible = true;
            //CountdownWrapper.BackgroundColor = Color.FromHex("4D71A3");
            //TimerWrapper.IsVisible = true;
            //CompleteWrapper.IsVisible = false;
            //NotifyWrapper.IsVisible = true;
            //ToggleAdButton(false);
        }

        void ShowLogOutState()
        {
            //CountdownWrapper.IsVisible = false;
            //NotifyWrapper.IsVisible = false;
            //LogInBtn.IsVisible = true;
            //WatchAdButton.IsVisible = false;
            //PointsLabel.Text = "0";
        }

        private void StartCooldownTimer(int days=0, int hours=0, int minutes=0, int seconds=0)
        {

            //CountDown rewardsCooldown = new CountDown { Date =
            //    new DateTime(DateTime.Now.Ticks + new TimeSpan(
            //            days, hours, minutes, seconds).Ticks
            //        ) };
            //Device.StartTimer(new TimeSpan(0, 0, 1), () =>
            //{

            //    if (!isLoggedIn)
            //    {
            //        // User logged out while on rewards page
            //        ShowLogOutState();
            //        return false;
            //    }

            //    var timespan = rewardsCooldown.Date - DateTime.Now;

            //    if (timespan.Ticks < 0)
            //    {
            //        ShowDoneBanner();
            //        return false;
            //    }

            //    rewardsCooldown.Timespan = timespan;
            //    HoursLabel.Text = rewardsCooldown.Hours;
            //    MinutesLabel.Text = rewardsCooldown.Minutes;
            //    SecondsLabel.Text = rewardsCooldown.Seconds;

            //    ShowTimerBanner();

            //    return true;
            //});
        }

        protected override void OnAppearing()
        {
            //MessagingCenter.Send(Application.Current, "UpdateToolbar", true);

            //#if DEBUG
            //    CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-3940256099942544/5224354917");
            //#else
            //    #if __IOS__
            //        CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-9382412071078825/4201400125");
            //    #elif __ANDROID__
            //        CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-9382412071078825/7152256279");
            //    #endif
            //#endif

            //if (!isLoggedIn)
            //{

            //    ShowLogOutState();

            //}
            //else
            //{
            //    LogInBtn.IsVisible = false;
            //    UpdateUserPoints();
            //}

        }

        public async void ClaimReward(string rewardAmount)
        {
            //try
            //{
            //    var values = new Dictionary<string, string>{
            //            { "username", currentUser },
            //            { "reward", rewardAmount }
            //        };

            //    var content = new FormUrlEncodedContent(values);
            //    var response = await App.globalClient.PostAsync(App.HolidailyHost + "/user/", content);
            //    var responseString = await response.Content.ReadAsStringAsync();
            //    dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            //    int status = responseJSON.status;
            //    string message = responseJSON.message;
            //    if (status == 200)
            //    {
            //        await DisplayAlert("Cha-ching!", "You've been awarded " + rewardAmount + " confetti!", "OK");
            //    }
            //    else
            //    {
            //        await DisplayAlert("Error", message, "OK");
            //    }
            //}
            //catch(Exception ex)
            //{
            //    Debug.WriteLine(ex);
            //}
            

        }
        public async void WatchAd(object sender, EventArgs e)
        {
            //if (!isLoggedIn)
            //{
            //    App.promptLogin(Navigation);
            //    return;
            //}
            //CrossMTAdmob.Current.ShowRewardedVideo();
        }
        public async void LogIn(object sender, EventArgs e)
        {
            //App.promptLogin(Navigation);
        }

        public async void NotifyToggled(object sender, ToggledEventArgs e)
        {
            //if (!isLoggedIn)
            //{
            //    App.promptLogin(Navigation);
            //    return;
            //}
            //var values = new Dictionary<string, string>{
            //            { "username", currentUser },
            //            { "notify_cooldown", e.Value.ToString() }
            //        };

            //var content = new FormUrlEncodedContent(values);
            //await App.globalClient.PostAsync(App.HolidailyHost + "/user/", content);
        }



    }
}