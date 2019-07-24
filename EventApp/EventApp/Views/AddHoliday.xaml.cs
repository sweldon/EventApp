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
    public partial class AddHoliday : ContentPage
    {

        HttpClient client = new HttpClient();


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

        public string isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
            set
            {
                if (Settings.IsLoggedIn == value)
                    return;
                Settings.IsLoggedIn = value;
                OnPropertyChanged();
            }
        }

        public AddHoliday()
        {
            InitializeComponent();

            BindingContext = this;
        }

        public async void SendHoliday(object sender, EventArgs e)
        {
            this.IsEnabled = false;

            if (isLoggedIn == "no")
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                this.IsEnabled = true;
            }
            else
            {

                if ( (!string.IsNullOrEmpty(HolidayDetailEntry.Text)) && (!string.IsNullOrEmpty(HolidayNameEntry.Text)))
                {
                    var selectedDate = HolidayDateEntry.Date;
                    var dateStr = selectedDate.ToString();

                    var values = new Dictionary<string, string>{
                           { "user", currentUser },
                           { "name", HolidayNameEntry.Text },
                           { "description", HolidayDetailEntry.Text },
                           { "date", dateStr },
                        };

                    var content = new FormUrlEncodedContent(values);
                    var response = await client.PostAsync(App.HolidailyHost + "/portal/add_holiday_from_app/", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                    int status = responseJSON.StatusCode;

                    if (status == 200)
                    {
                        SendHolidayBtn.Text = "Submission Pending...";
                        await DisplayAlert("Success", "We received your Holiday! We will get back to you ASAP about it's release into the App. Thank you for your contribution to Holidaily!", "OK");
                    }
                    else
                    {
                        this.IsEnabled = true;
                        await DisplayAlert("Error", "Something went wrong, please try again", "OK");
                    }
                }
                else
                {
                    this.IsEnabled = true;
                    await DisplayAlert("Try Again", "We need you to put someting into all of the fields on this page.", "OK");
                }



                

            }




        }

        public async void PendingAlert(object sender, EventArgs e)
        {
            await DisplayAlert("Hold Up!", "We're currently checking out your most recently submitted holiday. Please check back soon!", "OK");
        }

        protected override async void OnAppearing()
        {

            base.OnAppearing();

            if (isLoggedIn == "yes")
            {
                var values = new Dictionary<string, string>{
                   { "user", currentUser },
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(App.HolidailyHost + "/portal/check_user_holiday_pending/", content);
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                bool pending = responseJSON.pending;

                if (pending)
                {
                    SendHolidayBtn.IsEnabled = false;
                    SendHolidayBtn.Text = "Submission Pending...";

                    #if __IOS__
                        SendHolidayBtn.IsVisible = false;
                        PendingBtn.IsVisible = true;
                    #endif

                }
                else
                {
                    SendHolidayBtn.Text = "Send It";
                    SendHolidayBtn.IsEnabled = true;
                    #if __IOS__
                        SendHolidayBtn.IsVisible = true;
                        PendingBtn.IsVisible = false;
                    #endif
                }
            }

        }

    }
}
