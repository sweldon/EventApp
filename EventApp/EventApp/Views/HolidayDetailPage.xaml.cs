﻿using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using EventApp.Models;
using EventApp.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Xamarin.Essentials;
using System.Net.Http;
using Newtonsoft.Json;
#if __IOS__
using UIKit;
#endif
namespace EventApp.Views
{
    public partial class HolidayDetailPage : ContentPage
    {

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
            set
            {
                if (Settings.DevicePushId == value)
                    return;
                Settings.DevicePushId = value;
                OnPropertyChanged();
            }
        }

        HolidayDetailViewModel viewModel;

        public Comment Comment { get; set; }
        public HolidayDetailPage(HolidayDetailViewModel viewModel)
            {
                InitializeComponent();

                BindingContext = this.viewModel = viewModel;

            // Remove when reply button added
            HolidayDetailList.ItemSelected += OnCommentSelected;
            //HolidayDetailList.ItemTapped += (object sender, ItemTappedEventArgs e) =>
            //{
            //    // Attempt to disable highlighting
            //    if (sender is ListView lv) lv.SelectedItem = null;


            //};


        }


        async void OnDeleteTapped(object sender, EventArgs args)
        {

            if (isLoggedIn == "no")
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            }
            else
            {
                var item = (sender as Label).BindingContext as Comment;

                if (String.Equals(item.UserName, currentUser,
                       StringComparison.OrdinalIgnoreCase))
                {


                    var deleteComment = await DisplayAlert("Delete Forever",
                    "Are you sure you want to delete this comment?", "Yes", "No");
                    if (deleteComment)
                    {

                        var values = new Dictionary<string, string>{
                   { "comment_id", item.Id },
                   { "device_id", devicePushId }
                    };


                        var content = new FormUrlEncodedContent(values);
                        HttpClient client = new HttpClient();
                        var response = await client.PostAsync(App.HolidailyHost + "/portal/delete_comment/", content);

                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                        int status = responseJSON.status_code;
                        string message = responseJSON.message;
                        if (status == 200)
                        {
                            MessagingCenter.Send(this, "UpdateComments");
                            //await Navigation.PopAsync();
                        }
                        else
                        {
                            await DisplayAlert("Error", message, "OK");
                        }

                    }

                }

            }
        }

        async void OnReplyTapped(object sender, EventArgs args)
        {

            if (isLoggedIn == "no")
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            }
            else
            {
                var item = (sender as Label).BindingContext as Comment;
                await Navigation.PushAsync(new CommentPage(new CommentViewModel(item.Id, viewModel.Holiday.Id)));
            }
        }

        async void OnCommentSelected(object sender, SelectedItemChangedEventArgs args)
        {
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }

            var item = args.SelectedItem as Comment;
            //await Navigation.PushAsync(new CommentPage(new CommentViewModel(item.Id, viewModel.Holiday.Id)));

        }

        public HolidayDetailPage()
        {
            InitializeComponent();

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.GroupedCommentList.Count == 0)
                viewModel.LoadHolidayComments.Execute(null);
               

                viewModel.Holiday = await viewModel.HolidayStore.GetHolidayById(viewModel.HolidayId);
            if (!string.IsNullOrEmpty(viewModel.Holiday.Description))
                Description.Text = viewModel.Holiday.Description;
            else
                Description.Text = "This holiday has no information yet!";
            this.Title = viewModel.Holiday.Name;
            CurrentVotes.Text = viewModel.Holiday.Votes.ToString();
 
            if (isLoggedIn == "yes")
            {
                string currentVote = await viewModel.HolidayStore.CheckUserVotes(viewModel.HolidayId, currentUser);
                if(currentVote == "1" || currentVote == "4")
                {
                    UpVoteImage.Source = "up_active.png";
                    DownVoteImage.Source = "down.png";
                }
                else if(currentVote == "0" || currentVote == "5")
                {
                    DownVoteImage.Source = "down_active.png";
                    UpVoteImage.Source = "up.png";
                }
            }

        }

        async void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            if (isLoggedIn == "no")
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            }
            else {
                var labelSender = (Label)sender;
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new NewCommentPage(viewModel.Holiday)));
                this.IsEnabled = true;
            }
            this.IsEnabled = true;


        }

        async void OnShareTapped(object sender, EventArgs args)
        {
            this.IsEnabled = false;

            var labelSender = (Label)sender;
            this.IsEnabled = false;

            var holidayName = viewModel.Holiday.Name;
            string HolidayDescriptionShort = viewModel.Holiday.Description.Length <= 90 ? viewModel.Holiday.Description : viewModel.Holiday.Description.Substring(0, 90) + "...\nGet all the info with the Holidaily app! http://holidailyapp.com/applink";
            this.IsEnabled = false;
            string action = await DisplayActionSheet("How would you like to share?", "Cancel", null, "Text Message");
            if (action == "Text Message")
            {
                try
                {
                    var messageContents = holidayName + "! " + HolidayDescriptionShort;
                    var message = new SmsMessage(messageContents, "");
                    await Sms.ComposeAsync(message);
                }
                catch (FeatureNotSupportedException ex)
                {
                    // Sms is not supported on this device.
                }
                catch (Exception ex)
                {
                    // Other error has occurred.
                }
            }

            this.IsEnabled = true;



        }

        async void OnCommentPicTapped(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            if (isLoggedIn == "no")
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            }
            else
            {
                var labelSender = (Image)sender;
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new NewCommentPage(viewModel.Holiday)));
                this.IsEnabled = true;
            }
            this.IsEnabled = true;


        }

        async void OnSharePicTapped(object sender, EventArgs args)
        {
            this.IsEnabled = false;

            var buttonSender = (Image)sender;
            var holidayName = viewModel.Holiday.Name;
            string HolidayDescriptionShort = viewModel.Holiday.Description.Length <= 90 ? viewModel.Holiday.Description : viewModel.Holiday.Description.Substring(0, 90) + "...\nGet all the info with the Holidaily app! http://holidailyapp.com";
            this.IsEnabled = false;
            string action = await DisplayActionSheet("How would you like to share?", "Cancel", null, "Text Message");
            if (action == "Text Message")
            {
                try
                {
                    var messageContents = holidayName + "! "+ HolidayDescriptionShort;
                    var message = new SmsMessage(messageContents, "");
                    await Sms.ComposeAsync(message);
                }
                catch (FeatureNotSupportedException ex)
                {
                    // Sms is not supported on this device.
                }
                catch (Exception ex)
                {
                    // Other error has occurred.
                }
            }
            this.IsEnabled = true;
     

        }


        async void DownVote(object sender, EventArgs args)
        {

            #if __IOS__
                var haptic = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
                haptic.Prepare();
                haptic.ImpactOccurred();
                haptic.Dispose();
            #endif

            #if __ANDROID__
                var duration = TimeSpan.FromSeconds(.025);
                Vibration.Vibrate(duration);
            #endif

            string newVotes = CurrentVotes.Text;
            int newVotesInt = Int32.Parse(newVotes);
            var DownVoteImageFile = DownVoteImage.Source as FileImageSource;
            var DownVoteIcon = DownVoteImageFile.File;
            var UpVoteImageFile = UpVoteImage.Source as FileImageSource;
            var UpVoteIcon = UpVoteImageFile.File;

            if (isLoggedIn == "no")
            {
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                this.IsEnabled = true;
            }
            else
            {

                if (UpVoteIcon == "up_active.png")
                {
                    newVotesInt -= 2;
                    CurrentVotes.Text = newVotesInt.ToString();
                    UpVoteImage.Source = "up.png";
                    DownVoteImage.Source = "down_active.png";
                    await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "5");
                }
                else
                {
                    if (DownVoteIcon == "down_active.png")
                    {
                        // Undo
                        newVotesInt += 1;
                        CurrentVotes.Text = newVotesInt.ToString();
                        DownVoteImage.Source = "down.png";
                        await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "2");
                    }
                    else
                    {
                        // Only allow if user hasnt already downvoted
                        newVotesInt -= 1;
                        if (newVotesInt <= Int32.Parse(CurrentVotes.Text) + 1 && newVotesInt >= Int32.Parse(CurrentVotes.Text) - 1)
                        {
                            CurrentVotes.Text = newVotesInt.ToString();
                            DownVoteImage.Source = "down_active.png";
                            await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "0");
                        }
                        else
                        {
                            // Undo
                            newVotesInt += 2;
                            CurrentVotes.Text = newVotesInt.ToString();
                            DownVoteImage.Source = "down.png";
                            await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "4");
                        }
                    }
                }

            }



        }

        async void UpVote(object sender, EventArgs args)
        {
            #if __IOS__
                var haptic = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
                haptic.Prepare();
                haptic.ImpactOccurred();
                haptic.Dispose();
            #endif

            #if __ANDROID__
                var duration = TimeSpan.FromSeconds(.025);
                Vibration.Vibrate(duration);
            #endif

            string newVotes = CurrentVotes.Text;
            int newVotesInt = Int32.Parse(newVotes);
            var DownVoteImageFile = DownVoteImage.Source as FileImageSource;
            var DownVoteIcon = DownVoteImageFile.File;
            var UpVoteImageFile = UpVoteImage.Source as FileImageSource;
            var UpVoteIcon = UpVoteImageFile.File;

            if (isLoggedIn == "no")
            {
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                this.IsEnabled = true;
            }
            else
            {

                if (DownVoteIcon == "down_active.png")
                {
                    Debug.WriteLine("Down was active upvoting twice");
                    newVotesInt += 2;
                    CurrentVotes.Text = newVotesInt.ToString();
                    DownVoteImage.Source = "down.png";
                    UpVoteImage.Source = "up_active.png";
                    await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "4");
                }
                else
                {
                    if (UpVoteIcon == "up_active.png")
                    {
                        // Undo

                        newVotesInt -= 1;
                        CurrentVotes.Text = newVotesInt.ToString();
                        UpVoteImage.Source = "up.png";
                        await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "3");
                    }
                    else
                    {
                        // Only allow if user hasnt already downvoted
                        newVotesInt += 1;
                        if (newVotesInt <= Int32.Parse(CurrentVotes.Text) + 1 && newVotesInt >= Int32.Parse(CurrentVotes.Text) - 1)
                        {
                            CurrentVotes.Text = newVotesInt.ToString();
                            UpVoteImage.Source = "up_active.png";
                            await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "1");
                        }
                        else
                        {
                            newVotesInt -= 2;
                            CurrentVotes.Text = newVotesInt.ToString();
                            UpVoteImage.Source = "up.png";
                            await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "5");
                        }

                    }


                }
            }

        }


        async void DownVoteComment(object sender, EventArgs args)
        {
            //var voteStatus = (sender as Image).Source;
            var item = (sender as Image).BindingContext as Comment;
            string commentId = item.Id;

            #if __IOS__
                var haptic = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
                haptic.Prepare();
                haptic.ImpactOccurred();
                haptic.Dispose();
            #endif

            #if __ANDROID__
                var duration = TimeSpan.FromSeconds(.025);
                Vibration.Vibrate(duration);
            #endif

            int CurrentVotes = item.Votes;
            //var DownVoteImageFile = DownVoteImage.Source as FileImageSource;
            //var DownVoteIcon = DownVoteImageFile.File;
            //var UpVoteImageFile = UpVoteImage.Source as FileImageSource;
            //var UpVoteIcon = UpVoteImageFile.File;

            if (isLoggedIn == "no")
            {
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                this.IsEnabled = true;
            }
            else
            {

                if (item.UpVoteStatus == "up_active.png")
                {
                    item.Votes -= 2;
                    //CurrentVotes.Text = newVotesInt.ToString();
                    //UpVoteImage.Source = "up.png";
                    //DownVoteImage.Source = "down_active.png";
                    item.UpVoteStatus = "up.png";
                    item.DownVoteStatus = "down_active.png";
                    await viewModel.CommentStore.VoteComment(commentId, currentUser, "5");
                }
                else
                {
                    if (item.DownVoteStatus == "down_active.png")
                    {
                        // Undo
                        item.Votes += 1;
                        //CurrentVotes.Text = newVotesInt.ToString();
                        //DownVoteImage.Source = "down.png";
                        item.DownVoteStatus = "down.png";
                        await viewModel.CommentStore.VoteComment(commentId, currentUser, "2");
                    }
                    else
                    {
                        // Only allow if user hasnt already downvoted
                        int newVotes = item.Votes - 1;
                        if (newVotes <= item.Votes + 1 && newVotes >= item.Votes - 1)
                        {
                            item.Votes -= 1;
                            //CurrentVotes.Text = newVotesInt.ToString();
                            //DownVoteImage.Source = "down_active.png";
                            item.DownVoteStatus = "down_active.png";
                            await viewModel.CommentStore.VoteComment(commentId, currentUser, "0");
                        }
                        else
                        {
                            // Undo
                            item.Votes += 2;
                            //CurrentVotes.Text = newVotesInt.ToString();
                            //DownVoteImage.Source = "down.png";
                            item.DownVoteStatus = "down.png";
                            await viewModel.CommentStore.VoteComment(commentId, currentUser, "4");
                        }
                    }
                }

            }



        }

        async void UpVoteComment(object sender, EventArgs args)
        {

            var item = (sender as Image).BindingContext as Comment;
            string commentId = item.Id;

            #if __IOS__
                var haptic = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
                haptic.Prepare();
                haptic.ImpactOccurred();
                haptic.Dispose();
            #endif

            #if __ANDROID__
                var duration = TimeSpan.FromSeconds(.025);
                Vibration.Vibrate(duration);
            #endif

            int CurrentVotes = item.Votes;

            if (isLoggedIn == "no")
            {
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                this.IsEnabled = true;
            }
            else
            {

                if (item.DownVoteStatus == "down_active.png")
                {
                    Debug.WriteLine("Down was active upvoting twice");
                    item.Votes += 2;
                    //CurrentVotes.Text = newVotesInt.ToString();
                    //DownVoteImage.Source = "down.png";
                    // UpVoteImage.Source = "up_active.png";
                    item.DownVoteStatus = "down.png";
                    item.UpVoteStatus = "up_active.png";
                    await viewModel.CommentStore.VoteComment(commentId, currentUser, "4");
                }
                else
                {
                    if (item.UpVoteStatus == "up_active.png")
                    {
                        // Undo

                        item.Votes -= 1;
                        //CurrentVotes.Text = newVotesInt.ToString();
                        //UpVoteImage.Source = "up.png";
                        item.UpVoteStatus = "up.png";
                        await viewModel.CommentStore.VoteComment(commentId, currentUser, "3");
                    }
                    else
                    {
                        // Only allow if user hasnt already downvoted
                        int newVotes = item.Votes + 1;
                        if (newVotes <= item.Votes + 1 && newVotes >= item.Votes - 1)
                        {
                            item.Votes += 1;
                            //CurrentVotes.Text = newVotesInt.ToString();
                            //UpVoteImage.Source = "up_active.png";
                            item.UpVoteStatus = "up_active.png";
                            await viewModel.CommentStore.VoteComment(commentId, currentUser, "1");
                        }
                        else
                        {

                            item.Votes -= 2;
                            //CurrentVotes.Text = newVotesInt.ToString();
                            //UpVoteImage.Source = "up.png";
                            item.UpVoteStatus = "up.png";
                            await viewModel.CommentStore.VoteComment(commentId, currentUser, "5");
                        }

                    }


                }
            }

        }


    }
}
