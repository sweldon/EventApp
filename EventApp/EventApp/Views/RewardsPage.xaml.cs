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

            #if __IOS__
                CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-9382412071078825/4201400125");
            #endif

            #if __ANDROID__
                CrossMTAdmob.Current.LoadRewardedVideo("ca-app-pub-9382412071078825/7152256279");
            #endif
            

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