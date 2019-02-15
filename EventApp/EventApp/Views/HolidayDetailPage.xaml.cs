using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using EventApp.Models;
using EventApp.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
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



        HolidayDetailViewModel viewModel;

        public Comment Comment { get; set; }
        public HolidayDetailPage(HolidayDetailViewModel viewModel)
            {
                InitializeComponent();

                BindingContext = this.viewModel = viewModel;

                HolidayDetailList.ItemSelected += OnCommentSelected;

                //swipeContainer.Swipe += (sender, e) =>
                //{
                //    switch (e.Direction)
                //    {
                //        case SwipeDirection.Right:
                //            Navigation.PopAsync();
                //            break;
                //    }
                //};

        }


        async void OnCommentSelected(object sender, SelectedItemChangedEventArgs args)
        {
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            var item = args.SelectedItem as Comment;
            await Navigation.PushAsync(new CommentPage(new CommentViewModel(item.Id, viewModel.Holiday.Id)));

        }

        public HolidayDetailPage()
        {
            InitializeComponent();

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Comments.Count == 0)
                viewModel.LoadHolidayComments.Execute(null);

            viewModel.Holiday = await viewModel.HolidayStore.GetHolidayById(viewModel.HolidayId);
            Description.Text = viewModel.Holiday.Description;
            this.Title = viewModel.Holiday.Name;
            CurrentVotes.Text = viewModel.Holiday.Votes.ToString();

        }

        async void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {

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


        }


        async void DownVote(object sender, EventArgs args)
        {
            var duration = TimeSpan.FromSeconds(.025);
            Vibration.Vibrate(duration);
            string newVotes = CurrentVotes.Text;
            int newVotesInt = Int32.Parse(newVotes);
            var DownVoteImageFile = DownVoteImage.Source as FileImageSource;
            var DownVoteIcon = DownVoteImageFile.File;
            var UpVoteImageFile = UpVoteImage.Source as FileImageSource;
            var UpVoteIcon = UpVoteImageFile.File;

            if (UpVoteIcon == "up_active.png")
            {
                newVotesInt -= 2;
                CurrentVotes.Text = newVotesInt.ToString();
                UpVoteImage.Source = "up.png";
                DownVoteImage.Source = "down_active.png";
            }
            else
            {
                if (DownVoteIcon == "down_active.png")
                {
                    // Undo
                    newVotesInt += 1;
                    CurrentVotes.Text = newVotesInt.ToString();
                    DownVoteImage.Source = "down.png";
                }
                else
                {
                    // Only allow if user hasnt already downvoted
                    newVotesInt -= 1;
                    if (newVotesInt <= viewModel.Holiday.Votes + 1 && newVotesInt >= viewModel.Holiday.Votes - 1)
                    {
                        CurrentVotes.Text = newVotesInt.ToString();
                        DownVoteImage.Source = "down_active.png";
                    }
                    else
                    {
                        // Undo
                        newVotesInt += 2;
                        CurrentVotes.Text = newVotesInt.ToString();
                        DownVoteImage.Source = "down.png";
                    }
                }
            }



        }

        async void UpVote(object sender, EventArgs args)
        {
            var duration = TimeSpan.FromSeconds(.025);
            Vibration.Vibrate(duration);
            string newVotes = CurrentVotes.Text;
            int newVotesInt = Int32.Parse(newVotes);
            var DownVoteImageFile = DownVoteImage.Source as FileImageSource;
            var DownVoteIcon = DownVoteImageFile.File;
            var UpVoteImageFile = UpVoteImage.Source as FileImageSource;
            var UpVoteIcon = UpVoteImageFile.File;

            if (DownVoteIcon == "down_active.png")
            {
                Debug.WriteLine("Down was active upvoting twice");
                newVotesInt += 2;
                CurrentVotes.Text = newVotesInt.ToString();
                DownVoteImage.Source = "down.png";
                UpVoteImage.Source = "up_active.png";
            }
            else
            {
                if (UpVoteIcon == "up_active.png")
                {
                    // Undo
                    
                    newVotesInt -= 1;
                    CurrentVotes.Text = newVotesInt.ToString();
                    UpVoteImage.Source = "up.png";
                }
                else
                {
                    // Only allow if user hasnt already downvoted
                    newVotesInt += 1;
                    if (newVotesInt <= viewModel.Holiday.Votes + 1 && newVotesInt >= viewModel.Holiday.Votes - 1)
                    {
                        CurrentVotes.Text = newVotesInt.ToString();
                        UpVoteImage.Source = "up_active.png";
                    }
                    else
                    {
                        newVotesInt -= 2;
                        CurrentVotes.Text = newVotesInt.ToString();
                        UpVoteImage.Source = "up.png";
                    }

                }


            }

        }


    }
}
