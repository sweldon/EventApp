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
        string ec2Instance = "http://ec2-54-156-187-51.compute-1.amazonaws.com";
        CommentViewModel viewModel;
        public Comment Comment { get; set; }

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

        protected override void OnAppearing()
        {
            base.OnAppearing();

        }

        async void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {


        }

        public async Task SubmitReply(object sender, EventArgs e)
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
                   { "user", viewModel.currentUser}

                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(ec2Instance + "/portal/add_comment/", content);

                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int status = responseJSON.StatusCode;
                string message = responseJSON.Message;
                Debug.WriteLine("REPLY STATUS: "+status);
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
