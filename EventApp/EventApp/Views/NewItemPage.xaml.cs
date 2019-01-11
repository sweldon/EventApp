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
    public partial class NewItemPage : ContentPage
    {

        HttpClient client = new HttpClient();
        string ec2Instance = "http://ec2-54-156-187-51.compute-1.amazonaws.com";

        public Holiday OpenedHoliday { get; set; }
        public string CommentTitle { get; set; }

        public NewItemPage(Holiday holiday)
        {
            InitializeComponent();

            OpenedHoliday = holiday;
            CommentTitle = "Comment on " + OpenedHoliday.Name;
            BindingContext = this;
        }

        public async Task SubmitComment(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            if (string.IsNullOrEmpty(CommentContent.Text))
            {
                await DisplayAlert("Nothing to say?", "You have to type something to say something...", "Well okay.");
                this.IsEnabled = true;
            }
            else {
                var values = new Dictionary<string, string>{
                   { "holiday_id", OpenedHoliday.Id },
                   { "comment", CommentContent.Text }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(ec2Instance + "/portal/add_comment/", content);

                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int status = responseJSON.StatusCode;

                if (status == 200)
                {
                    await Navigation.PopModalAsync();
                    this.IsEnabled = true;
                    MessagingCenter.Send(this, "UpdateComments");
                }
                else
                {
                    await DisplayAlert("Error", "Something went wrong on our end. We couldn't save your comment.", "Try again");
                    this.IsEnabled = true;
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
