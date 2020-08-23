using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using Plugin.Media.Abstractions;
using Plugin.Media;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddHoliday : ContentPage
    {

        private MediaFile UploadedMedia;
        private bool initialCheck = false;
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
            SendHolidayBtn.IsEnabled = false;
            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
                SendHolidayBtn.IsEnabled = true;
            }
            else
            {
                if ( (!string.IsNullOrEmpty(HolidayDetailEntry.Text)) && (!string.IsNullOrEmpty(HolidayNameEntry.Text)))
                {
                    try
                    {
                        var selectedDate = HolidayDateEntry.Date;
                        var dateStr = selectedDate.ToString();
                        MultipartFormDataContent content = new MultipartFormDataContent();

                        if (UploadedImage.IsVisible)
                        {
                            Stream streamedImage = UploadedMedia.GetStream();
                            content.Add(new StreamContent(streamedImage), "file", $"{UploadedMedia.Path}");
                        }

                        StringContent username = new StringContent(currentUser);
                        content.Add(username, "username");
                        content.Add(new StringContent(HolidayNameEntry.Text), "submission");
                        content.Add(new StringContent(HolidayDetailEntry.Text), "description");
                        content.Add(new StringContent(dateStr), "date");
                        var response = await App.globalClient.PostAsync(App.HolidailyHost + "/submit/", content);
                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                        int status = responseJSON.status;

                        if (status == 200)
                        {
                            PendingWrapper.IsVisible = true;
                            CheckStatusWrapper.IsVisible = true;
                            SubmissionWrapper.IsVisible = false;

                            HolidayNameEntry.Text = "";
                            HolidayDetailEntry.Text = "";
                            UploadedImage.IsVisible = false;
                        }
                        else
                        {
                            await DisplayAlert("Error", "Something went wrong, please try again.", "OK");
                        }
                    }
                    catch(Exception ex)
                    {
                        await DisplayAlert("Error", "Something went wrong. Please try again.", "OK");
                        Debug.WriteLine($"{ex}");
                    }
                }
                else
                {
                    await DisplayAlert("Try Again", "Please enter something into all fields", "OK");
                }
                SendHolidayBtn.IsEnabled = true;
            }


        }

        public async void PendingAlert(object sender, EventArgs e)
        {
            await DisplayAlert("Hold Up!", "We're currently checking out your most recently submitted holiday. Please check back soon!", "OK");
        }


        async void UploadImage(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Not Supported", "Uploading pictures" +
                    " doesn't seem to be supported by your device.", "OK");
                return;
            }
            var mediaOptions = new PickMediaOptions()
            {
                PhotoSize = PhotoSize.Small
            };
            var selectedImageFile = await CrossMedia.Current.PickPhotoAsync(mediaOptions);
            if (selectedImageFile == null)
            {
                await DisplayAlert("Uh oh!", "We couldn't upload that pic. " +
                    "Please try again.", "OK");
                return;
            }

            UploadedImage.Source = ImageSource.FromStream(() => selectedImageFile.GetStreamWithImageRotatedForExternalStorage());
            UploadedImage.IsVisible = true;
            UploadedMedia = selectedImageFile;

        }

        private async void CheckStatus(object sender, EventArgs e)
        {
            CheckStatusBtn.IsEnabled = false;
            CheckStatusBtn.TextColor = Color.FromHex("E0E0E0");
            CheckPending(manual: true);
            await Task.Delay(10000);
            CheckStatusBtn.TextColor = Color.FromHex("FFFFFF");
            CheckStatusBtn.IsEnabled = true;
        }

        async void CheckPending(bool manual=false)
        {
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
                        PendingWrapper.IsVisible = true;
                        CheckStatusWrapper.IsVisible = true;
                        SubmissionWrapper.IsVisible = false;
                        if (manual)
                        {
                            await DisplayAlert("Holiday Status",
                                "Your holiday is still under review", "OK");
                        }
                    }
                    else
                    {
                        PendingWrapper.IsVisible = false;
                        CheckStatusWrapper.IsVisible = false;
                        SubmissionWrapper.IsVisible = true;
                    }

                    if (!initialCheck)
                    {
                        HolidayNameEntry.Text = "";
                        HolidayDetailEntry.Text = "";
                        UploadedImage.IsVisible = false;
                    }

                }
                catch
                {
                    await DisplayAlert("Error", "Couldn't connect to Holidaily", "OK");
                }

            }
            else
            {
                //PendingWrapper.IsVisible = false;
                //CheckStatusWrapper.IsVisible = false;
                //SubmissionWrapper.IsVisible = true;
            }
        }

        protected override async void OnAppearing()
        {

            base.OnAppearing();
            MessagingCenter.Send(Application.Current, "UpdateToolbar", true);
            if (!initialCheck)
            {
                initialCheck = true;
                CheckPending();
            }


        }

    }
}
