using EventApp.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EventApp.Models;
using System.Collections.ObjectModel;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfettiLeaders : ContentPage
    {

        public ObservableCollection<User> TopConfettiList { get; set; }
        public Command LoadUsers { get; set; }
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
            Title = "Top 20 Confetti Leaders";

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            viewModel.LoadUsers.Execute(null);
            AdBanner.IsVisible = !isPremium;
        } 

    }
}