using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace EventApp.Views
{
    public partial class PortalPage : ContentPage
    {

        public bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
        }
        public PortalPage()
        {
            InitializeComponent();
        }

        async void Cancel(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Navigation.PopModalAsync();
            this.IsEnabled = true;
        }
        async void Register(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Navigation.PushModalAsync(new NavigationPage(new RegisterPage()));
            this.IsEnabled = true;
        }
        async void Login(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            this.IsEnabled = true;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (isLoggedIn)
            {
                await Navigation.PopModalAsync();
            }
        }
    }
}
