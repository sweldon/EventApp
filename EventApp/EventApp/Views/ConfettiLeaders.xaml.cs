using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventApp.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EventApp.Models;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfettiLeaders : ContentPage
    {

        HttpClient client = new HttpClient();
        public ObservableCollection<User> TopConfettiList { get; set; }
        public Command LoadUsers { get; set; }

        bool IsBusy;
        public bool isPremium
        {
            get { return Settings.IsPremium; }
            set
            {
                if (Settings.IsPremium == value)
                    return;
                Settings.IsPremium = value;
                OnPropertyChanged();
            }
        }
        

        ConfettiLeaderViewModel viewModel;

        public ConfettiLeaders()
        {
            InitializeComponent();
            BindingContext = viewModel = new ConfettiLeaderViewModel();
            Title = "Confetti Leaders";
            ConfettiLeadersList.ItemSelected += Selected;

        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            viewModel.LoadUsers.Execute(null);
            AdBanner.IsVisible = !isPremium;
        }

        async void Selected(object sender, SelectedItemChangedEventArgs args)
        {
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }

        }

 

    }
}