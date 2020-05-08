using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using EventApp.ViewModels;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace EventApp.Views
{
    public partial class ProfilePage : ContentPage
    {
        ProfileViewModel viewModel;
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
        public ProfilePage(ProfileViewModel viewModel, bool fromMenu=false)
        {
            InitializeComponent();
            BindingContext = this.viewModel = viewModel;
            if (!fromMenu)
            {
                ToolbarItems.RemoveAt(0);
            }
            if(App.GlobalUser.Avatar != null)
            {
                ProfilePicture.Source = App.GlobalUser.Avatar;
            }
            else
            {
                ProfilePicture.Source = "default_user_128.png";
            }
            Confetti.Text = App.GlobalUser.Confetti;
            Comments.Text = App.GlobalUser.Comments;
            Holidays.Text = App.GlobalUser.Approved;
            UserName.Text = App.GlobalUser.UserName;
            goPremiumButton.IsVisible = !isPremium;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
        }
        private async void OpenProfile(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Navigation.PushAsync(new UserPage(user: App.GlobalUser));
            this.IsEnabled = true;
        }
        async void Back(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Navigation.PopModalAsync();
            this.IsEnabled = true;
        }
        async void ChooseUploadType(object sender, EventArgs e)
        {
            var choosePic = await DisplayAlert("Edit Profile Picture", "Would " +
                "you like to upload from your gallery, or take a new picture?", "Upload Picture", "Take Picture");
            if (choosePic)
            {
                ChooseAvatar();
            }
            else
            {
                TakePicture();
            }
        }
        async void UploadAvatar(MediaFile ProfilePic)
        {
            ImageSource newPicture = ImageSource.FromStream(() => ProfilePic.GetStreamWithImageRotatedForExternalStorage());
            MessagingCenter.Send(this, "UpdateMenuProfilePicture", newPicture);
            var stream = ProfilePic.GetStreamWithImageRotatedForExternalStorage();
            var bytes = new byte[stream.Length];
            MultipartFormDataContent content = new MultipartFormDataContent();
            ByteArrayContent baContent = new ByteArrayContent(bytes);
            StringContent username = new StringContent(currentUser);
            content.Add(new StreamContent(ProfilePic.GetStream()), "file", $"{ProfilePic.Path}");
            content.Add(username, "username");
            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/user/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            string avatar = responseJSON.avatar;
            App.GlobalUser.Avatar = avatar;
            ProfilePicture.Source = avatar;
        }
        async void ChooseAvatar()
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
            UploadAvatar(selectedImageFile);
        }
        async void TakePicture()
        {
            try
            {
                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("Not Supported", "Taking pictures" +
                        " doesn't seem to be supported by your device.", "OK");
                    return;
                }
                var mediaOptions = new StoreCameraMediaOptions()
                {
                    PhotoSize = PhotoSize.Small,
                    AllowCropping = false
                };
                var selectedImageFile = await CrossMedia.Current.TakePhotoAsync(mediaOptions);
                if (selectedImageFile == null)
                {
                    await DisplayAlert("Uh oh!", "We couldn't upload that pic. " +
                        "Please try again.", "OK");
                    return;
                }
                UploadAvatar(selectedImageFile);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Camera Permission", "Have you previously denied" +
                    " our request to use the camera? Please allow us to" +
                    " use your camera to take a profile picture in the Holidaily" +
                    " section of your phone settings.", "OK");
            }
            await CrossMedia.Current.Initialize();

        }

        public async void OpenPremium(object sender, EventArgs e)
        {
            goPremiumButton.IsEnabled = false;
            await Navigation.PushAsync(new Premium());
            goPremiumButton.IsEnabled = true;
        }
    }
}
