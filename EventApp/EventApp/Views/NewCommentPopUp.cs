using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EventApp.Models;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;

namespace EventApp.Views
{
    public partial class NewCommentPopUp : Rg.Plugins.Popup.Pages.PopupPage
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
        private int overflowCount = 0;
        private bool overflow = false;
        public Holiday OpenedHoliday { get; set; }
        public string CommentTitle { get; set; }
        public ObservableCollection<User> users { get; set; }
        private Comment ReplyComment { get; set; }
        private bool isSearching = false;
        private bool isReply = false;
        public NewCommentPopUp(Holiday holiday, Comment comment=null)
        {
            InitializeComponent();
            users = new ObservableCollection<User>();
            OpenedHoliday = holiday;
            UserMentionList.ItemSelected += OnUserSelected;
            //UserMentionList.ItemTapped += ItemTapped;
            CommentContent.Placeholder = "Say something";
            if (comment != null)
            {
                // Replying
                ReplyComment = comment;
                isReply = true;
                ReplyWrapper.IsVisible = true;
                UserName.Text = comment.UserName;
                TimeSince.Text = comment.TimeSince;
                ReplyContent.Text = comment.Content;
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

        private async void CheckMentions(object sender, TextChangedEventArgs e)
        {
            users.Clear();
            // TODO just have to figure out how to go back down
            #if __IOS__
                if(CommentContent.Height > 100 && !overflow)
                {
                    CommentContent.AutoSize = EditorAutoSizeOption.Disabled;
                    overflowCount = CommentContent.Text.Length;
                    overflow = true;                      
                }
                if(CommentContent.Text.Length < overflowCount)
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

                    if(userList.Count > 0)
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
            else
            {
                var values = new Dictionary<string, string>{
                   { "holiday", OpenedHoliday.Id },
                   { "content", CommentContent.Text },
                   { "username", currentUser}
                };
                if (isReply)
                {
                    values["parent"] = ReplyComment.Id;
                }

                var content = new FormUrlEncodedContent(values);
                var response = await App.globalClient.PostAsync(App.HolidailyHost + "/comments/", content);

                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                int status = responseJSON.status;
                string message = responseJSON.message;

                if (status == 200)
                {
                    //await Navigation.PopModalAsync();
                    await Navigation.PopPopupAsync();
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


        protected override async void OnAppearing()
        {

            base.OnAppearing();
            await Task.Delay(100);
            CommentContent.Focus();

            if(isReply)
                CommentContent.Text = $"@{ReplyComment.UserName} ";
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

    }
}