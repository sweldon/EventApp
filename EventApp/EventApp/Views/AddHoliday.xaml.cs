using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using Newtonsoft.Json;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddHoliday : ContentPage
    {


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

        public bool isLoggedIn
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

            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
                this.IsEnabled = true;
            }
            else
            {

                if ( (!string.IsNullOrEmpty(HolidayDetailEntry.Text)) && (!string.IsNullOrEmpty(HolidayNameEntry.Text)))
                {
                    try
                    {
                        var selectedDate = HolidayDateEntry.Date;
                        var dateStr = selectedDate.ToString();

                        var values = new Dictionary<string, string>{
                           { "username", currentUser },
                           { "submission", HolidayNameEntry.Text },
                           { "description", HolidayDetailEntry.Text },
                           { "date", dateStr },
                        };

                        var content = new FormUrlEncodedContent(values);
                        var response = await App.globalClient.PostAsync(App.HolidailyHost + "/submit/", content);
                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                        int status = responseJSON.status;

                        if (status == 200)
                        {
                            SendHolidayBtn.Text = "Submission Pending...";
                            await DisplayAlert("Success", "We received your Holiday! We will get back to you ASAP about its release into the App. Thank you for your contribution to Holidaily!", "OK");
                        }
                        else
                        {
                            this.IsEnabled = true;
                            await DisplayAlert("Error", "Something went wrong, please try again", "OK");
                        }
                    }
                    catch
                    {
                        await DisplayAlert("Error", "Something went wrong. Please try again.", "OK");
                        this.IsEnabled = true;
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
            //MessagingCenter.Send(this, "UpdateToolbar", true);
            if (isLoggedIn)
            {
                try
                {
                    var values = new Dictionary<string, string>{
                        { "username", currentUser },
                    };

                    var content = new FormUrlEncodedContent(values);
                    var response = await App.globalClient.PostAsync(App.HolidailyHost + "/pending/", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                    bool pending = responseJSON.results;

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
                catch
                {
                    await DisplayAlert("Error", "Couldn't connect to Holidaily", "OK");
                }

            }

        }

    }
}
