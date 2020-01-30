using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using EventApp.Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LimboPage : ContentPage
    {


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

        public LimboPage()
        {
            InitializeComponent();

            BindingContext = this;

            UserName.Text = currentUser;
        }



        public async void SwitchAccounts(object sender, EventArgs e)
        {
            isLoggedIn = false;
            await DisplayAlert("Cool", "This account has been removed. Please relaunch the app.", "OK");
        }


    }
}
