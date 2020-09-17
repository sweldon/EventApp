using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventApp.Models;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Stormlion.PhotoBrowser;
using Xamarin.Forms;

namespace EventApp.Views
{
    public partial class PostPage : ContentPage
    {
        private MediaFile UploadedMedia;
        private string ImagePath;
        private bool ImageRemoved = false;
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
        }
        private Holiday Holiday;
        private bool isEdit;
        private Post EditedPost;
        public PostPage(Holiday holiday, Post post=null)
        {
            InitializeComponent();
            Holiday = holiday;
            if (post != null)
            {
                isEdit = true;
                EditedPost = post;

                CommentContent.Text = $"{EditedPost.Content} ";
                if (!string.IsNullOrEmpty(EditedPost.Image))
                {
                    UploadedImage.Source = EditedPost.Image;
                    UploadedImage.IsVisible = true;
                    RemoveImageButton.IsVisible = true;
                }
                Avatar.Source = App.GlobalUser.Avatar == null ? "default_user_128.png" : EditedPost.Avatar;
                UserName.Text = EditedPost.UserName;
            }
            else
            {
                Avatar.Source = App.GlobalUser.Avatar == null ? "default_user_128.png" : App.GlobalUser.Avatar;
                UserName.Text = currentUser;
            }

        }

        async void GoBack(object sender, EventArgs e)
        {
            BackBtn.IsEnabled = false;
            await Navigation.PopModalAsync();
            await Task.Delay(2000);
            BackBtn.IsEnabled = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();


        }

        async void AddPost(object sender, EventArgs e)
        {
            PostBtn.IsEnabled = false;
            PostBtn.Text = "Posting...";

            MultipartFormDataContent content = new MultipartFormDataContent();

            if(!UploadedImage.IsVisible && string.IsNullOrEmpty(CommentContent.Text))
            {
                await DisplayAlert("Content Required",
                    "Please either type a comment or add an image", "OK");
                PostBtn.IsEnabled = true;
                PostBtn.Text = "Post";
                return;
            }

            if (UploadedMedia != null)
            {
                Stream streamedImage = UploadedMedia.GetStream();
                content.Add(new StreamContent(streamedImage), "post_image", $"{UploadedMedia.Path}");
            }
            if(!string.IsNullOrEmpty(CommentContent.Text))
            {
                content.Add(new StringContent(CommentContent.Text), "content");
            }
            content.Add(new StringContent(Holiday.Id), "holiday_id");
            content.Add(new StringContent(currentUser), "username");
            dynamic responseJSON;
            string postId;
            if (isEdit)
            {
                if (ImageRemoved)
                {
                    content.Add(new StringContent("1"), "clear_image");
                }
                content.Add(new StringContent(devicePushId), "device_id");
                var response = await App.globalClient.PatchAsync($"{App.HolidailyHost}/posts/{EditedPost.Id}/", content);
                if (response.IsSuccessStatusCode)
                {
                    MessagingCenter.Send(this, "RefreshPosts");
                    await Navigation.PopModalAsync();
                }
                else
                {
                    Debug.WriteLine($"ERROR: {response.StatusCode}");
                }
               
                
            }
            else
            {
                var response = await App.globalClient.PostAsync(App.HolidailyHost + "/posts/", content);
                var responseString = await response.Content.ReadAsStringAsync();
                responseJSON = JsonConvert.DeserializeObject(responseString);
                postId = responseJSON.post_id;

                string image = responseJSON.image;
                bool ShowImage = string.IsNullOrEmpty(image) ? false : true;
                await Navigation.PopModalAsync();
                Post post = new Post()
                {
                    Id = postId,
                    Content = CommentContent.Text,
                    HolidayId = Holiday.Id,
                    UserName = currentUser,
                    TimeSince = "now",
                    ShowReply = "false",
                    ShowEdit = "true", // If you can delete, you can edit
                    ShowDelete = "true",
                    ShowReport = false,
                    Likes = 0,
                    Avatar = App.GlobalUser.Avatar == null ?
                   "default_user_128.png" : App.GlobalUser.Avatar,
                    Image = image,
                    ShowImage = ShowImage
                };
                MessagingCenter.Send(this, "AddPost", post);
            }


            await Task.Delay(3000);
            PostBtn.IsEnabled = true;
            PostBtn.Text = "Post";
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
            ImagePath = selectedImageFile.Path;
            UploadedImage.Source = ImageSource.FromStream(() => selectedImageFile.GetStreamWithImageRotatedForExternalStorage());
            UploadedImage.IsVisible = true;
            RemoveImageButton.IsVisible = true;
            UploadedMedia = selectedImageFile;
            CommentContent.Focus();

        }

        async void TakePhoto(object sender, EventArgs e)
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
                    AllowCropping = true
                };
                var selectedImageFile = await CrossMedia.Current.TakePhotoAsync(mediaOptions);
                if (selectedImageFile == null)
                {
                    await DisplayAlert("Uh oh!", "We couldn't upload that pic. " +
                        "Please try again.", "OK");
                    return;
                }
                ImagePath = selectedImageFile.Path;
                UploadedImage.Source = ImageSource.FromStream(() => selectedImageFile.GetStreamWithImageRotatedForExternalStorage());
                UploadedImage.IsVisible = true;
                RemoveImageButton.IsVisible = true;
                UploadedMedia = selectedImageFile;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Camera Permission", "Have you previously denied" +
                    " our request to use the camera? Please allow us to" +
                    " use your camera to take a profile picture in the Holidaily" +
                    " section of your phone settings.", "OK");
            }
            await CrossMedia.Current.Initialize();
            CommentContent.Focus();
        }

        void RemoveImage(object sender, EventArgs e)
        {
            UploadedImage.IsVisible = false;
            RemoveImageButton.IsVisible = false;
            UploadedMedia = null;
            CommentContent.Focus();
            ImageRemoved = true;
        }



    }
}
