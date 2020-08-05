using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EventApp.Models;
using EventApp.Views;
using EventApp.ViewModels;
using System.Diagnostics;
using Xamarin.Essentials;
using Plugin.Share;
using Plugin.Share.Abstractions;
using System.Collections.ObjectModel;
using Stormlion.PhotoBrowser;
using FFImageLoading.Forms;

#if __IOS__
using UIKit;
#endif

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HolidaysPage : ContentPage
    {
        HolidaysViewModel viewModel;

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

        public bool OpenNotifications
        {
            get { return Settings.OpenNotifications; }
            set
            {
                if (Settings.OpenNotifications == value)
                    return;
                Settings.OpenNotifications = value;
                OnPropertyChanged();
            }
        }

        public int notifCount
        {
            get { return Settings.NotificationCount; }
            set
            {
                if (Settings.NotificationCount == value)
                    return;
                Settings.NotificationCount = value;
                OnPropertyChanged();
            }
        }

        public bool isActive
        {
            get { return Settings.IsActive; }
            set
            {
                if (Settings.IsActive == value)
                    return;
                Settings.IsActive = value;
                OnPropertyChanged();
            }
        }

        public string showAds;
        public ObservableCollection<Tweet> Tweets { get; set; }
        
        public HolidaysPage()
        {
            InitializeComponent();
            Tweets = new ObservableCollection<Tweet>();
            BindingContext = viewModel = new HolidaysViewModel();
            ItemsListView.ItemTapped += (object sender, ItemTappedEventArgs e) => {
                if (e.Item == null) return;
                Task.Delay(500);
                if (sender is ListView lv) lv.SelectedItem = null;
                };

            // Twitter feed infinite scroll
            // Infinite scrolling for comments
            SocialMediaList.ItemAppearing += (sender, e) =>
            {

                //if (viewModel.IsBusy || viewModel.GroupedCommentList.Count == 0)
                //{
                //    return;
                //}
                //LoadingCommentsDialog.IsVisible = true;
                var tweet = e.Item as Tweet;
                if (Tweets.Last() == tweet)
                {
                    LoadMoreTweets();
                }
                //LoadingCommentsDialog.IsVisible = false;
            };

            // List highlighting
            SocialMediaList.ItemTapped += (object sender, ItemTappedEventArgs e) => {
                // don't do anything if we just de-selected the row.
                if (e.Item == null) return;

                // Optionally pause a bit to allow the preselect hint.
                Task.Delay(500);

                // Deselect the item.
                if (sender is ListView lv) lv.SelectedItem = null;

  
                };

        }

        public int page = 0;
        public bool allLoaded = false;
        public async void LoadMoreTweets()
        {
            //LoadingDialog.IsVisible = true;
            //if (!allLoaded)
            //{
            page += 1;
            ObservableCollection<Tweet> moreTweets = await Services.GlobalServices.GetTweets(page);
            foreach(Tweet t in moreTweets)
            {
                Tweets.Add(t);
            }
            //LoadingDialog.IsVisible = false;
        }

        async void ImageToHoliday(object sender, EventArgs args)
        {
            var item = new Holiday();
            try
            {
                item = (sender as ContentView).BindingContext as Holiday;
            }
            catch
            {
                item = (sender as Image).BindingContext as Holiday;
            }
            
            string holidayId = item.Id;
            if (holidayId != "-1") // Ad
            {
                this.IsEnabled = false;
                await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId, item)));
                this.IsEnabled = true;
            }
        }

        async void LabelToHoliday(object sender, EventArgs args)
        {

            var item = (sender as Label).BindingContext as Holiday;
            string holidayId = item.Id;
            if (holidayId != "-1") // Ad
            {
                this.IsEnabled = false;
                await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId, item)));
                this.IsEnabled = true;
            }
        }

        public async void OpenInTwitter(object sender, EventArgs e)
        {
            var tweet = (sender as StackLayout).BindingContext as Tweet;
            Device.BeginInvokeOnMainThread(() => {
                Device.OpenUri(new Uri(tweet.Url));
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Send(Application.Current, "UpdateToolbar", true);

            // Get new notifications on first launch
            if (!App.NotificationsRefreshed)
            {
                try
                {
                    notifCount = await Utils.GetUserNotificationCount();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Could not sync user data: {ex.Message}");
                }
            }
            
            
            MessagingCenter.Unsubscribe<HolidayDetailPage, Object[]>(this, "UpdateCelebrateStatus");
            // When logging in from menu we need to refresh the feed statuses
            MessagingCenter.Unsubscribe<LoginPage>(this, "UpdateHolidayFeed");
            MessagingCenter.Subscribe<MenuPage>(this, "UpdateHolidayFeed", (sender) => {
                Debug.WriteLine("Refreshing all holidays");
                viewModel.ExecuteLoadItemsCommand();
            });

            if (viewModel.Holidays.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);


            int numRetries = 0;
            while(viewModel.Holidays.Count == 0)
            {
                numRetries++;
                if (numRetries >= 15) {
                    await DisplayAlert("Error", "Couldn't connect to Holidaily", "OK");
                    return;
                }
                await Task.Delay(2000);
            }
    
            if (OpenNotifications)
            {
                await Navigation.PushModalAsync(new NavigationPage(new NotificationsPage()));
                OpenNotifications = false;
            }

            if (App.syncDeviceToken)
            {
                try
                {
                    Utils.syncUser();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Could not sync user data: {ex.Message}");
                }
                
            }
            AdBanner.IsVisible = !isPremium;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Subscribe<LoginPage>(this, "UpdateHolidayFeed", (sender) => {
                Debug.WriteLine("Refreshing all holidays");
                viewModel.ExecuteLoadItemsCommand();
            });
            MessagingCenter.Unsubscribe<MenuPage>(this, "UpdateHolidayFeed");
            MessagingCenter.Subscribe<HolidayDetailPage, Object[]>(this,
            "UpdateCelebrateStatus", (sender, data) => {
                viewModel.UpdateCelebrateStatus((string)data[0], (bool)data[1], (string)data[2]);
            });

            App.syncDeviceToken = false;
        }

        async void OnCelebrateTapped(object sender, EventArgs args)
        {
            this.IsEnabled = false;
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
            
            var view = (VisualElement)sender;
            var GridObject = (Grid)view.Parent;
            var CelebrateImage = (Image)GridObject.Children[4];
            var UpVoteImageFile = CelebrateImage.Source as FileImageSource;
            var UpVoteIcon = UpVoteImageFile.File;
            
            var holidayId = "none";
            var votesInt = -1;
            var CelebrateLabel = (Label)GridObject.Children[5];
            var holiday = new Holiday();
            try
            {
                holiday = (sender as Label).BindingContext as Holiday;
                votesInt = Int32.Parse((sender as Label).Text.Split(null)[0]);
                holidayId = holiday.Id;
            }
            catch
            {
                holiday = (sender as Image).BindingContext as Holiday;
                votesInt = Int32.Parse(CelebrateLabel.Text.Split(null)[0]);
                holidayId = holiday.Id;
            }

            var newVotes = votesInt;

            if (!isLoggedIn)
            {
                this.IsEnabled = false;
                App.promptLogin(Navigation);
                this.IsEnabled = true;
            }
            else
            {
                if (UpVoteIcon == "celebrate_active.png")
                {
                    // Undo
                    newVotes -= 1;
                    holiday.CelebrateStatus = "celebrate.png";
                    holiday.Votes = newVotes.ToString();
                    await CelebrateImage.ScaleTo(2, 50);
                    await CelebrateImage.ScaleTo(1, 50);
                    await viewModel.HolidayStore.VoteHoliday(holidayId, "3");

                }
                else
                {
                    // Only allow if user hasnt already downvoted
                    newVotes += 1;
                    if (newVotes <= votesInt + 1 && newVotes >= votesInt - 1)
                    {
                        holiday.CelebrateStatus = "celebrate_active.png";
                        holiday.Votes = newVotes.ToString();
                        await CelebrateImage.ScaleTo(2, 50);
                        await CelebrateImage.ScaleTo(1, 50);
                        await viewModel.HolidayStore.VoteHoliday(holidayId, "1");

                    }
                    else
                    {
                        newVotes -= 2;
                        holiday.CelebrateStatus = "celebrate.png";
                        holiday.Votes = newVotes.ToString();
                        await CelebrateImage.ScaleTo(2, 50);
                        await CelebrateImage.ScaleTo(1, 50);
                        await viewModel.HolidayStore.VoteHoliday(holidayId, "5");

                    }

                }
            }

            this.IsEnabled = true;

        }

        public void Share(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            var holiday = new Holiday();
            try
            {
                holiday = (sender as Label).BindingContext as Holiday;
            }
            catch
            {
                holiday = (sender as Image).BindingContext as Holiday;
            }

            var holidayName = holiday.Name;
            var holidayLink = App.HolidailyHost+"/holiday?id=" + holiday.Id;
            string blurb = $"{holidayName}! {holiday.Blurb}\nCheck it out on Holidaily!";

            if (!CrossShare.IsSupported)
                return;

            CrossShare.Current.Share(new ShareMessage
            {
                Title = holidayName,
                Text = blurb,
                Url = holidayLink
            });

            this.IsEnabled = true;

        }


        protected void RefreshSocialMediaCommand(object sender, EventArgs e)
        {
            page = 0;
            RefreshSocialMediaFeed();
            SocialMediaList.EndRefresh();
            NoResults.IsVisible = Tweets.Count == 0 ? true : false;
        }

        protected void RefreshHolidaysCommand(object sender, EventArgs e)
        {
            viewModel.LoadItemsCommand.Execute(null);
            ItemsListView.EndRefresh();
        }

        private async void RefreshSocialMediaFeed()
        {

            try
            {
                //tweets.Clear();
                Tweets = await Services.GlobalServices.GetTweets();
                SocialMediaList.ItemsSource = Tweets;
                NoResults.IsVisible = Tweets.Count == 0 ? true : false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async void ToggleFindType(object sender, EventArgs args)
        {

            var contentViewSender = (ContentView)sender;
            var labelSender = (Label)contentViewSender.Children[0];
            var searchType = labelSender.Text;
            if (searchType == TodayLabel.Text)
            {
                HolidayListWrapper.IsVisible = true;
                SocialMediaWrapper.IsVisible = false;
                TodaySelected.IsVisible = true;
                SocialMediaSelected.IsVisible = false;
                TodayLabel.TextColor = Color.FromHex("4c96e8");
                SocialMediaLabel.TextColor = Color.Gray;
            }
            else
            {
                if (Tweets.Count == 0)
                    RefreshSocialMediaFeed();

                SocialMediaWrapper.IsVisible = true;
                HolidayListWrapper.IsVisible = false;
                SocialMediaSelected.IsVisible = true;
                TodaySelected.IsVisible = false;
                TodayLabel.TextColor = Color.Gray;
                SocialMediaLabel.TextColor = Color.FromHex("4c96e8");
            }
        }
        public async void PreviewImage(object sender, EventArgs args)
        {
            try
            {
                var tweet = (sender as CachedImage).BindingContext as Tweet;
                string tweetUrl = tweet.Image;
                new PhotoBrowser
                {
                    Photos = new List<Photo>
                {
                    new Photo
                    {
                        URL = tweetUrl,
                        Title = $"{tweet.Handle}'s Photo on Twitter"
                    }
                }
                }.Show();
            }
            catch
            {
                await DisplayAlert("Ouch!", "Sorry, we couldn't load this" +
                    " image at the moment", "OK");
            }

        }

    }
}