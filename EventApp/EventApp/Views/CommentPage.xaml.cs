using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using EventApp.Models;
using EventApp.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CommentPage : ContentPage
    {


        HttpClient client = new HttpClient();
        CommentViewModel viewModel;
        public Comment Comment { get; set; }

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

        public string devicePushId
        {
            get { return Settings.DevicePushId; }
            set
            {
                if (Settings.DevicePushId == value)
                    return;
                Settings.DevicePushId = value;
                OnPropertyChanged();
            }
        }

        public CommentPage(CommentViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = this.viewModel = viewModel;

        }


        async void OnCommentSelected(object sender, SelectedItemChangedEventArgs args)
        {
            ((ListView)sender).SelectedItem = null;

        }

        public CommentPage()
        {
            InitializeComponent();

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            ReplyCommentContent.Focus();

            viewModel.Comment = await viewModel.CommentStore.GetCommentById(viewModel.CommentId);

            //Description.Text = viewModel.Holiday.Description;
            //this.Title = viewModel.Holiday.Name;
            string UserNameValue = viewModel.Comment.UserName;
            Content.Text = viewModel.Comment.Content;
            TimeSince.Text = viewModel.Comment.TimeSince;
            UserName.Text = UserNameValue;
            this.Title = viewModel.Comment.UserName + "'s Comment"; 
            int UserNameLength = UserNameValue.Length;
            ReplyCommentContent.Text = '@'+UserNameValue.PadRight(UserNameLength + 1, ' ');

            if (String.Equals(currentUser, UserNameValue, StringComparison.OrdinalIgnoreCase))
            {
                DeleteContentView.IsVisible = true;
            }
            else
            {
                ReplyContentView.IsVisible = true;
                ReplyTextContentView.IsVisible = true;
            }

        }

        async void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {


        }

        public async void DeleteComment(object sender, EventArgs e)
        {
            var deleteComment = await DisplayAlert("Delete Forever", 
            "Are you sure you want to delete this comment?", "Yes", "No");
            if (deleteComment)
            {

                var values = new Dictionary<string, string>{
                   { "comment_id", viewModel.CommentId },
                   { "device_id", devicePushId }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(App.HolidailyHost + "/portal/delete_comment/", content);

                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int status = responseJSON.status_code;
                string message = responseJSON.message;
                if (status == 200)
                {
                    MessagingCenter.Send(this, "UpdateComments");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", message, "OK");
                }

            }
        }

        public async void SubmitReply(object sender, EventArgs e)
        {
            SaveReplyButton.IsEnabled = false;
            SaveReplyButton.Text = "Replying...";
            if (string.IsNullOrEmpty(ReplyCommentContent.Text))
            {
                await DisplayAlert("Nothing to say?", "You have to type something to say something...", "Well okay.");
                SaveReplyButton.IsEnabled = true;
                SaveReplyButton.Text = "Reply";
            }
            else
            {
                var values = new Dictionary<string, string>{
                   { "holiday_id", viewModel.HolidayId },
                   { "comment", ReplyCommentContent.Text },
                   { "user", viewModel.currentUser},
                   {"parent", viewModel.CommentId }

                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(App.HolidailyHost + "/portal/add_comment/", content);

                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int status = responseJSON.StatusCode;
                string message = responseJSON.Message;
                if (status == 200)
                {
                    await Navigation.PopAsync();
                    SaveReplyButton.IsEnabled = true;
                    SaveReplyButton.Text = "Reply";
                    MessagingCenter.Send(this, "UpdateComments");
                }
                else
                {
                    await DisplayAlert("Error", message, "OK");
                    SaveReplyButton.IsEnabled = true;
                    SaveReplyButton.Text = "Reply";
                }
            }


        }


    }
}
