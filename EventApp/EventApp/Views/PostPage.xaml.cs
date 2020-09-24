using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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
        private int overflowCount = 0;
        private bool isSearching = false;
        private bool overflow = false;
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
        public ObservableCollection<User> users { get; set; }
        ContentView Container;
        public PostPage(Holiday holiday, Post post=null, ContentView container =null)
        {
            InitializeComponent();
            Container = container;
            users = new ObservableCollection<User>();
            UserMentionList.ItemSelected += OnUserSelected;
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
                Stream streamedImage = UploadedMedia.GetStreamWithImageRotatedForExternalStorage();
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
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic postJSON = JsonConvert.DeserializeObject(responseString);
                    var TimeSinceEdit = $"{postJSON.time_since} (edited now)";
                    Object[] data = { EditedPost, CommentContent.Text, TimeSinceEdit, Container };
                    MessagingCenter.Send(this, "UpdatePostInPlace", data);
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
                Post NewPost = new Post()
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
                    ShowImage = ShowImage,
                    LikeEnabled = true,
                    LikeTextColor = Color.FromHex("808080"),
                };

                MessagingCenter.Send(this, "AddPost", NewPost);
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
            UploadedImage.Source = ImageSource.FromStream(() => selectedImageFile.GetStream());
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
                UploadedImage.Source = ImageSource.FromStream(() => selectedImageFile.GetStream());
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


        private async void CheckMentions(object sender, TextChangedEventArgs e)
        {
            users.Clear();

            #if __IOS__
                if (CommentContent.Height > 100 && !overflow)
                {
                    CommentContent.AutoSize = EditorAutoSizeOption.Disabled;
                    overflowCount = CommentContent.Text.Length;
                    overflow = true;
                }
                if (CommentContent.Text.Length < overflowCount)
                {
                    CommentContent.AutoSize = EditorAutoSizeOption.TextChanges;
                    overflow = false;
                }
            #endif

            // Sync width for autosizing
            CommentContent.WidthRequest = CommentContent.Width;


            UserMentionList.ItemsSource = users;
            // Nothing entered
            if (CommentContent.Text.Length == 0)
            {
                UserMentionPlaceHolderWrapper.IsVisible = false;
                UserMentionListWrapper.IsVisible = false;
                isSearching = false;
                return;
            }
            // Sentence terminator
            else if (new Regex(@"[.?!\s]").IsMatch(CommentContent.Text.Substring(CommentContent.Text.Length - 1)))
            {
                UserMentionPlaceHolderWrapper.IsVisible = false;
                UserMentionListWrapper.IsVisible = false;
                NoResults.IsVisible = false;
                isSearching = false;
                return;
            }
            // Mention beginning
            if (CommentContent.Text.Substring(CommentContent.Text.Length - 1) == "@")
            {
                isSearching = true;
                UserMentionPlaceHolderWrapper.IsVisible = true;
                NoResults.IsVisible = false;
                UserMentionListWrapper.IsVisible = false;
                UserMentionListWrapper.HeightRequest = 70;
            }

            var match = Regex.Match(CommentContent.Text, @".+@([^\s]+)");
            // First word
            if (!CommentContent.Text.Contains(" "))
            {
                match = Regex.Match(CommentContent.Text, @"@([^\s]+)");
            }
            string searchText = match.Groups[match.Groups.Count - 1].ToString();

            if (searchText.Length > 15)
                return;

            if (isSearching && searchText != "" && CommentContent.Text.Substring(CommentContent.Text.Length - 1) != "@")
            {
                try
                {

                    var response = await App.globalClient.GetAsync(App.HolidailyHost + $"/users/?search={searchText}");
                    var responseString = await response.Content.ReadAsStringAsync();

                    dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                    dynamic userList = responseJSON.results;

                    if (userList.Count > 0)
                    {
                        UserMentionListWrapper.IsVisible = true;
                        UserMentionPlaceHolderWrapper.IsVisible = false;
                        NoResults.IsVisible = false;
                    }

                    if (userList.Count > 1)
                    {
                        UserMentionListWrapper.HeightRequest = 140;
                    }
                    else if (userList.Count == 1)
                    {
                        UserMentionListWrapper.HeightRequest = 70;
                    }
                    else
                    {
                        UserMentionListWrapper.IsVisible = false;
                        UserMentionPlaceHolderWrapper.IsVisible = false;
                        NoResults.IsVisible = true;
                    }


                    foreach (var user in userList)
                    {
                        var avatar = user.profile_image == null ? "default_user_128.png" : user.profile_image;
                        users.Add(new User()
                        {
                            UserName = user.username,
                            Avatar = avatar,
                        });

                    }

                    UserMentionList.ItemsSource = users;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex}");
                }
            }


        }


        async void OnUserSelected(object sender, SelectedItemChangedEventArgs args)
        {

            #if __ANDROID__
                CommentContent.Focus();
            #endif
            var SelectedUser = args.SelectedItem as User;
            string username = SelectedUser.UserName;

            CommentContent.Text = Regex.Replace(CommentContent.Text,
                @"\B\@([\w\-]+)$", $"@{username} ");

        }


    }
}
