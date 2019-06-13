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
    public partial class Premium : ContentPage
    {

        HttpClient client = new HttpClient();

        public Holiday OpenedHoliday { get; set; }
        public string CommentTitle { get; set; }

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

        public Premium()
        {
            InitializeComponent();

            BindingContext = this;
        }


        async void MakePurchase(object sender, EventArgs e)
        {
            await DisplayAlert("Soon!", "Premium isn't quite ready yet, but is coming soon. We will send you a notification when it is ready.", "I'll come back later!");
        }

        protected override async void OnAppearing()
        {

            base.OnAppearing();

        }

    }
}
