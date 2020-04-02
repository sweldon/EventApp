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
namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationsPage : ContentPage
    {
        NotificationsViewModel viewModel;
        Comment comment;

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
        

        public NotificationsPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new NotificationsViewModel(); ;
            Title = "Notifications";
    
            NotificationsList.ItemSelected += OnItemSelected;
        }

        async void GoBack(object sender, EventArgs e)
        {
            BackBtn.IsEnabled = false;
            await Navigation.PopModalAsync();
            await Task.Delay(2000);
            BackBtn.IsEnabled = true;
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            this.IsEnabled = false;
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            var notif = args.SelectedItem as Notification;

            if (notif.Type == "Comment")
            {
                try
                {
                    //await Navigation.PushAsync(new CommentPage(new CommentViewModel(notif.Id, comment.HolidayId)));
                    comment = await viewModel.CommentStore.GetCommentById(notif.Id);
                    Holiday holiday = await viewModel.HolidayStore.GetHolidayById(comment.HolidayId);
                    await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(comment.HolidayId, holiday, notif.Id)));
                }
                catch
                {
                    await DisplayAlert("Uh oh!", "We could no longer find that comment.", "Fine");
                    this.IsEnabled = true;
                }
            }
            this.IsEnabled = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Notifications.Count == 0)
                viewModel.LoadNotifications.Execute(null);
            AdBanner.IsVisible = !isPremium;

        }

    }
}