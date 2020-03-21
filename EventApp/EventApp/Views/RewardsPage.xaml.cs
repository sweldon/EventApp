using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MarcTron.Plugin;
using System.Net.Http;
using Newtonsoft.Json;

namespace EventApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RewardsPage : ContentPage
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

        public RewardsPage ()
		{
			InitializeComponent ();

            // Test ad: ca-app-pub-3940256099942544/5224354917

            CrossMTAdmob.Current.OnRewardedVideoStarted += (object sender, EventArgs e) => {
                WatchAdButton.Text = "Watch Ad for a Reward!";
                WatchAdButton.IsEnabled = true;
            };

            CrossMTAdmob.Current.OnRewardedVideoAdLoaded += (object sender, EventArgs e) => {
                WatchAdButton.Text = "Watch Ad for a Reward!";
                WatchAdButton.IsEnabled = true;
            };

            CrossMTAdmob.Current.OnRewardedVideoAdClosed += (object sender, EventArgs e) => {
                WatchAdButton.Text = "Watch Ad for a Reward!";
                WatchAdButton.IsEnabled = true;
            };

            CrossMTAdmob.Current.OnRewarded += (object sender, MTEventArgs e) => {

                ClaimReward("5");
                WatchAdButton.Text = "Watch Another Ad for More Rewards!";
                WatchAdButton.IsEnabled = true;
                
            };


        }

        public async void UpdateUserPoints()
        {
            try
            {
                var values = new Dictionary<string, string>{
                { "username", currentUser }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await App.globalClient.PostAsync(App.HolidailyHost + "/user/", content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                string points = responseJSON.results.ContainsKey("confetti") ? responseJSON.results.confetti.ToString() : "0";
                PointsLabel.Text = "You have " + points + " points!";
            }
            catch
            {
                await DisplayAlert("Error", "Couldn't connect to Holidaily", "OK");
            }

        }

        protected override void OnAppearing()
        {
            #if __IOS__
                        CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-9382412071078825/4201400125");
            #endif

            #if __ANDROID__
                        CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-9382412071078825/7152256279");
            #endif

            if (!isLoggedIn)
            {
                PointsLabel.Text = "Log in to get points!";
  
            }
            else
            {
        
                UpdateUserPoints();
            }
        }

        public async void ClaimReward(string rewardAmount)
        {
            var values = new Dictionary<string, string>{
                        { "username", currentUser },
                        { "reward", rewardAmount }
                    };

            var content = new FormUrlEncodedContent(values);
            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/user/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            int status = responseJSON.status;
            string message = responseJSON.message;
            if (status == 200)
            {
                await DisplayAlert("Cha-ching!", "You've been awarded "+rewardAmount+" points!", "OK");
                UpdateUserPoints();
            }
            else
            {
                await DisplayAlert("Error", message, "OK");
            }
        }
        public async void WatchAd(object sender, EventArgs e)
        {

            if (!isLoggedIn)
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            }
            else
            {
                WatchAdButton.Text = "Loading video...";
                CrossMTAdmob.Current.ShowRewardedVideo();
            }

    
        }


    }
}