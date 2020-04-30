using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EventApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class About : ContentPage
	{
		public About ()
		{
			InitializeComponent ();
		}

        async void ViewEula(object sender, EventArgs e)
        {
			await Navigation.PushAsync(new Eula());
		}

        public async void OpenInstagram(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                Xamarin.Forms.Device.OpenUri(new Uri("https://www.instagram.com/holidailyapp"));
            });
            this.IsEnabled = true;
        }
        public async void OpenFacebook(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                Xamarin.Forms.Device.OpenUri(new Uri("https://www.facebook.com/holidailyapp"));
            });
            this.IsEnabled = true;
        }
        public async void OpenTwitter(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                Xamarin.Forms.Device.OpenUri(new Uri("https://twitter.com/Holidaily_app"));
            });
            this.IsEnabled = true;
        }
        public async void OpenStore(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            #if __IOS__
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                    Xamarin.Forms.Device.OpenUri(new Uri("https://apps.apple.com/us/app/holidaily-find-holidays/id1449681401"));
                });
            #else
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                    Xamarin.Forms.Device.OpenUri(new Uri("https://play.google.com/store/apps/details?id=com.divinity.holidailyapp"));
                });
            #endif
            this.IsEnabled = true;
        }
    }
}