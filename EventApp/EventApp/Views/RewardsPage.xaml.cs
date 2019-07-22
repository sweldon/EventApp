using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MarcTron.Plugin;
using System.Diagnostics;
using EventApp.Services;
using System.Net.Http;
using Newtonsoft.Json;

namespace EventApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RewardsPage : ContentPage
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

                ClaimReward("video", "5");
                WatchAdButton.Text = "Watch Another Ad for More Rewards!";
                WatchAdButton.IsEnabled = true;
                
            };


        }

        public async void UpdateUserPoints()
        {
            var values = new Dictionary<string, string>{
            { "user", currentUser }
            };

            var content = new FormUrlEncodedContent(values);
            HttpClient client = new HttpClient();
            var response = await client.PostAsync(App.HolidailyHost + "/portal/get_user_rewards/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

            int points = responseJSON.Points;
            PointsLabel.Text = "You have " + points.ToString() + " points!";
        }

        protected async override void OnAppearing()
        {
            #if __IOS__
                        CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-9382412071078825/4201400125");
            #endif

            #if __ANDROID__
                        CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-9382412071078825/7152256279");
            #endif

            if (isLoggedIn == "no")
            {
                PointsLabel.Text = "Log in to get points!";
  
            }
            else
            {
        
                UpdateUserPoints();
            }
        }

        public async void ClaimReward(string rewardType, string rewardAmount)
        {
            var values = new Dictionary<string, string>{
                        { "type", rewardType },
                        { "user", currentUser },
                        { "amount", rewardAmount }
                    };

            var content = new FormUrlEncodedContent(values);
            HttpClient client = new HttpClient();
            var response = await client.PostAsync(App.HolidailyHost + "/portal/claim_reward/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            int status = responseJSON.StatusCode;
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

            if (isLoggedIn == "no")
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            }
            else
            {
                this.IsEnabled = false;
                WatchAdButton.Text = "Loading video...";
                CrossMTAdmob.Current.ShowRewardedVideo();
        
            }

    
        }


    }
}