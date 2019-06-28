using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MarcTron.Plugin;
using System.Diagnostics;

namespace EventApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RewardsPage : ContentPage
	{
		public RewardsPage ()
		{
			InitializeComponent ();

            // Test ad: ca-app-pub-3940256099942544/5224354917
            CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-1517355594758692/1318353970");

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


        }
        protected override void OnAppearing()
        {
           
        }
        public async void WatchAd(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            WatchAdButton.Text = "Loading video...";
            CrossMTAdmob.Current.ShowRewardedVideo();
    
        }


    }
}