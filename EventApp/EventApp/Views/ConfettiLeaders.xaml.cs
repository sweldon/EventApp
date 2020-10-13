using EventApp.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EventApp.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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
        public bool CommentLock;
        public ConfettiLeaders()
        {
            InitializeComponent();
            BindingContext = viewModel = new ConfettiLeaderViewModel();
            Title = "Top 20";
            ConfettiLeadersList.ItemSelected += OnItemSelected;
            CommentLock = false;
        }


        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {

            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            var SelectedUser = args.SelectedItem as User;

            if (!CommentLock)
            {
                CommentLock = true;
                try
                {
                    await Navigation.PushAsync(new UserPage(SelectedUser));
                }
                catch
                {
                    await DisplayAlert("Uh oh!", "Couldn't load this user's profile", "OK");
                }
                await Task.Delay(2000);
                CommentLock = false;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Send(Application.Current, "UpdateToolbar", true);
            if (viewModel.UserList.Count == 0)
                viewModel.LoadUsers.Execute(null);
            //AdBanner.IsVisible = !isPremium;

        }

    }
}