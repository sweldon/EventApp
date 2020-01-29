using System;
using Xamarin.Forms;
using EventApp.Models;
using EventApp.ViewModels;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Plugin.Share;
using Plugin.Share.Abstractions;

#if __IOS__
using UIKit;
#endif
namespace EventApp.Views
{
    public partial class HolidayDetailPage : ContentPage
    {



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

            if (!isLoggedIn)
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
                           { "delete", item.Id },
                           { "device_id", devicePushId }
                            };

                        var content = new FormUrlEncodedContent(values);
                        var response = await App.globalClient.PostAsync(App.HolidailyHost + "/comments/", content);

                        var responseString = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(responseString);
                        dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                        int status = responseJSON.status;
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

            if (!isLoggedIn)
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
            AdBanner.IsVisible = !isPremium;

            viewModel.Holiday = await viewModel.HolidayStore.GetHolidayById(viewModel.HolidayId);
            HolidayImageSource.Source = viewModel.Holiday.HolidayImage;
            if (!string.IsNullOrEmpty(viewModel.Holiday.Description))
                Description.Text = viewModel.Holiday.Description;
            else
                Description.Text = "This holiday has no information yet!";
            this.Title = viewModel.Holiday.Name;
            CurrentVotes.Text = viewModel.Holiday.Votes.ToString() + " Celebrating!";

            if (isLoggedIn) {
                UpVoteImage.Source = viewModel.Holiday.CelebrateStatus;
            }

            //if (!isLoggedIn)
            //{
            //    string currentVote = await viewModel.HolidayStore.CheckUserVotes(viewModel.HolidayId, currentUser);
            //    if (currentVote == "1" || currentVote == "4")
            //    {
            //        UpVoteImage.Source = "celebrate_active.png";
            //        //DownVoteImage.Source = "down.png";
            //    }
            //    else if (currentVote == "0" || currentVote == "5")
            //    {
            //        //DownVoteImage.Source = "down_active.png";
            //        UpVoteImage.Source = "celebrate.png";
            //    }
            //}
        }

        async void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            if (!isLoggedIn)
            {
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            }
            else
            {
                var labelSender = (Label)sender;
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new NewCommentPage(viewModel.Holiday)));
                this.IsEnabled = true;
            }
            this.IsEnabled = true;


        }

        public void Share(object sender, EventArgs args)
        {
            this.IsEnabled = false;

            var holiday = viewModel.Holiday;

            var holidayName = holiday.Name;
            var timeSince = holiday.Date;
            var holidayLink = "https://holidailyapp.com/holiday?id=" + holiday.Id;
            string preface = "It's " + holidayName + "! ";
            string HolidayDescriptionShort = holiday.Description.Length <= 90 ? preface + holiday.Description + "\nSee more! " : preface + holiday.Description.Substring(0, 90) + "...\nSee more! ";

            if (!CrossShare.IsSupported)
                return;

            CrossShare.Current.Share(new ShareMessage
            {
                Title = holidayName,
                Text = HolidayDescriptionShort,
                Url = holidayLink
            });

            this.IsEnabled = true;

        }

        async void AddComment(object sender, EventArgs args)
        {
            
            this.IsEnabled = false;
            int daysAgo = Time.ActiveHoliday(viewModel.Holiday.Date);
            if(daysAgo < 8)
            {
                if (!isLoggedIn)
                {
                    await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                }
                else
                {
                    this.IsEnabled = false;
                    await Navigation.PushModalAsync(new NavigationPage(new NewCommentPage(viewModel.Holiday)));
                    this.IsEnabled = true;
                }
            }
            else
            {
                await DisplayAlert("Sorry!", "We currently restrict new comments to holidays in the past week.", "OK");
            }
            this.IsEnabled = true;

        }


//        async void DownVote(object sender, EventArgs args)
//        {

//#if __IOS__
//                var haptic = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
//                haptic.Prepare();
//                haptic.ImpactOccurred();
//                haptic.Dispose();
//#endif

//#if __ANDROID__
//            var duration = TimeSpan.FromSeconds(.025);
//            Vibration.Vibrate(duration);
//#endif

//            string newVotes = CurrentVotes.Text;
//            int newVotesInt = Int32.Parse(newVotes);
//            var DownVoteImageFile = DownVoteImage.Source as FileImageSource;
//            var DownVoteIcon = DownVoteImageFile.File;
//            var UpVoteImageFile = UpVoteImage.Source as FileImageSource;
//            var UpVoteIcon = UpVoteImageFile.File;

//            if (!isLoggedIn)
//            {
//                this.IsEnabled = false;
//                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
//                this.IsEnabled = true;
//            }
//            else
//            {

//                if (UpVoteIcon == "up_active.png")
//                {
//                    newVotesInt -= 2;
//                    CurrentVotes.Text = newVotesInt.ToString();
//                    UpVoteImage.Source = "up.png";
//                    DownVoteImage.Source = "down_active.png";
//                    await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "5");
//                }
//                else
//                {
//                    if (DownVoteIcon == "down_active.png")
//                    {
//                        // Undo
//                        newVotesInt += 1;
//                        CurrentVotes.Text = newVotesInt.ToString();
//                        DownVoteImage.Source = "down.png";
//                        await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "2");
//                    }
//                    else
//                    {
//                        // Only allow if user hasnt already downvoted
//                        newVotesInt -= 1;
//                        if (newVotesInt <= Int32.Parse(CurrentVotes.Text) + 1 && newVotesInt >= Int32.Parse(CurrentVotes.Text) - 1)
//                        {
//                            CurrentVotes.Text = newVotesInt.ToString();
//                            DownVoteImage.Source = "down_active.png";
//                            await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "0");
//                        }
//                        else
//                        {
//                            // Undo
//                            newVotesInt += 2;
//                            CurrentVotes.Text = newVotesInt.ToString();
//                            DownVoteImage.Source = "down.png";
//                            await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "4");
//                        }
//                    }
//                }

//            }



//        }

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
                                    Xamarin.Essentials.Vibration.Vibrate(duration);
            #endif

            string VotesString = CurrentVotes.Text;
            int votesInt = Int32.Parse(VotesString.Split(null)[0]);
            //int newVotesInt = Int32.Parse(newVotes);
            int newVotesInt = Int32.Parse(VotesString.Split(null)[0]);
            //var DownVoteImageFile = DownVoteImage.Source as FileImageSource;
            //var DownVoteIcon = DownVoteImageFile.File;
            var UpVoteImageFile = UpVoteImage.Source as FileImageSource;
            var UpVoteIcon = UpVoteImageFile.File;


            if (!isLoggedIn)
            {
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                this.IsEnabled = true;
            }
            else
            {

                //if (DownVoteIcon == "down_active.png")
                //{
                //    Debug.WriteLine("Down was active upvoting twice");
                //    newVotesInt += 2;
                //    CurrentVotes.Text = newVotesInt.ToString() + " Celebrating!";
                //    //DownVoteImage.Source = "down.png";
                //    UpVoteImage.Source = "celebrate_active.png";
                //    await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, currentUser, "4");
                //}
                //else
                //{
                    if (UpVoteIcon == "celebrate_active.png")
                    {
                        // Undo

                        newVotesInt -= 1;
                        CurrentVotes.Text = newVotesInt.ToString() + " Celebrating!";
                        UpVoteImage.Source = "celebrate.png";
                        await UpVoteImage.ScaleTo(2, 50);
                        await UpVoteImage.ScaleTo(1, 50);
                        await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, "3");


                        Object[] values = { viewModel.Holiday.Name, false, newVotesInt.ToString() };
                        MessagingCenter.Send(this, "UpdateCelebrateStatus", values);
                }
                    else
                    {
                        // Only allow if user hasnt already downvoted
                        newVotesInt += 1;
                        if (newVotesInt <= votesInt + 1 && newVotesInt >= votesInt - 1)
                        {
                            CurrentVotes.Text = newVotesInt.ToString() + " Celebrating!";
                            UpVoteImage.Source = "celebrate_active.png";
                            await UpVoteImage.ScaleTo(2, 50);
                            await UpVoteImage.ScaleTo(1, 50);
                            await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, "1");


                        Object[] values = { viewModel.Holiday.Name, true, newVotesInt.ToString() };
                            MessagingCenter.Send(this, "UpdateCelebrateStatus", values);
                    }
                        else
                        {
                            newVotesInt -= 2;
                            CurrentVotes.Text = newVotesInt.ToString() + " Celebrating!";
                            UpVoteImage.Source = "celebrate.png";
                            await UpVoteImage.ScaleTo(2, 50);
                            await UpVoteImage.ScaleTo(1, 50);
                            await viewModel.HolidayStore.VoteHoliday(viewModel.HolidayId, "5");

    
                            Object[] values = { viewModel.Holiday.Name, false, newVotesInt.ToString() };
                            MessagingCenter.Send(this, "UpdateCelebrateStatus", values);

                    }

                    }


               //}
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
            Xamarin.Essentials.Vibration.Vibrate(duration);
#endif

            int CurrentVotes = item.Votes;
            //var DownVoteImageFile = DownVoteImage.Source as FileImageSource;
            //var DownVoteIcon = DownVoteImageFile.File;
            //var UpVoteImageFile = UpVoteImage.Source as FileImageSource;
            //var UpVoteIcon = UpVoteImageFile.File;

            if (!isLoggedIn)
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
                    await viewModel.CommentStore.VoteComment(commentId, "5");
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
                        await viewModel.CommentStore.VoteComment(commentId, "2");
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
                            await viewModel.CommentStore.VoteComment(commentId, "0");
                        }
                        else
                        {
                            // Undo
                            item.Votes += 2;
                            //CurrentVotes.Text = newVotesInt.ToString();
                            //DownVoteImage.Source = "down.png";
                            item.DownVoteStatus = "down.png";
                            await viewModel.CommentStore.VoteComment(commentId,"4");
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
                    Xamarin.Essentials.Vibration.Vibrate(duration);
        #endif

            int CurrentVotes = item.Votes;

            if (!isLoggedIn)
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
                    await viewModel.CommentStore.VoteComment(commentId, "4");
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
                        await viewModel.CommentStore.VoteComment(commentId, "3");
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
                            await viewModel.CommentStore.VoteComment(commentId, "1");
                        }
                        else
                        {

                            item.Votes -= 2;
                            //CurrentVotes.Text = newVotesInt.ToString();
                            //UpVoteImage.Source = "up.png";
                            item.UpVoteStatus = "up.png";
                            await viewModel.CommentStore.VoteComment(commentId, "5");
                        }

                    }


                }
            }

        }


    }
}
