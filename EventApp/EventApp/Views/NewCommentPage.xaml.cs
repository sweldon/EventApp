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
    public partial class NewCommentPage : ContentPage
    {

        HttpClient client = new HttpClient();
        string ec2Instance = "http://ec2-54-156-187-51.compute-1.amazonaws.com";

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

        public NewCommentPage(Holiday holiday)
        {
            InitializeComponent();

            OpenedHoliday = holiday;
            CommentTitle = "Comment on " + OpenedHoliday.Name;
            BindingContext = this;
        }

        public async void SubmitComment(object sender, EventArgs e)
        {
            SaveCommentButton.IsEnabled = false;
            SaveCommentButton.Text = "Posting...";
            if (string.IsNullOrEmpty(CommentContent.Text))
            {
                await DisplayAlert("Nothing to say?", "You have to type something to say something...", "Well okay.");
                SaveCommentButton.IsEnabled = true;
                SaveCommentButton.Text = "Post";
            }
            else {
                var values = new Dictionary<string, string>{
                   { "holiday_id", OpenedHoliday.Id },
                   { "comment", CommentContent.Text },
                   { "user", currentUser}

                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(ec2Instance + "/portal/add_comment/", content);

                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int status = responseJSON.StatusCode;
                string message = responseJSON.Message;

                if (status == 200)
                {
                    await Navigation.PopModalAsync();
                    SaveCommentButton.IsEnabled = true;
                    SaveCommentButton.Text = "Post";
                    MessagingCenter.Send(this, "UpdateComments");
                }
                else
                {
                    await DisplayAlert("Error", message, "OK");
                   SaveCommentButton.IsEnabled = true;
                   SaveCommentButton.Text = "Post";
                }
            }


        }

        async void Cancel_Clicked(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Navigation.PopModalAsync();
            this.IsEnabled = true;
        }
    }
}
