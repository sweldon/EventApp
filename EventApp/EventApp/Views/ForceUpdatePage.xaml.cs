using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace EventApp.Views
{
    public partial class ForceUpdatePage : ContentPage
    {
        public ForceUpdatePage()
        {
            InitializeComponent();
        }


        async void Update(object sender, EventArgs e)
        {
            
            #if __IOS__
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                    Xamarin.Forms.Device.OpenUri(new Uri("https://apps.apple.com/us/app/holidaily-find-holidays/id1449681401"));
                });
            #else
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                    Xamarin.Forms.Device.OpenUri(new Uri("https://play.google.com/store/apps/details?id=com.divinity.holidailyapp"));
                    });
            #endif
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}
