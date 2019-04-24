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

        public NotificationsPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new NotificationsViewModel(); ;
            Title = "Your Recent Activity";
    
            NotificationsList.ItemSelected += OnItemSelected;
        }


        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            var notif = args.SelectedItem as Notification;

            if(notif.Type == "Comment")
            {
                comment = await viewModel.CommentStore.GetCommentById(notif.Id);
                if(comment == null)
                {
                    await DisplayAlert("Uh oh!", "The author has deleted this comment", "Fine");
                }
                else
                {
                    await Navigation.PushAsync(new CommentPage(new CommentViewModel(notif.Id, comment.HolidayId)));
                }
                
            }

            

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Notifications.Count == 0)
                viewModel.LoadNotifications.Execute(null);


        }

    }
}