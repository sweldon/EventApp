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
//using Stormlion.PhotoBrowser;
using FFImageLoading.Forms;
using System.Collections;
using Rg.Plugins.Popup.Extensions;
using System.Net.Http;

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

        public string showAds;
        private bool todayDone = false;
        public ObservableCollection<Post> Posts { get; set; }

        public HolidaysPage()
        {
            InitializeComponent();
            Posts = new ObservableCollection<Post>();
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
                var post = e.Item as Post;
                if (Posts.Last() == post)
                {
                    LoadMorePosts();
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

            // Infinite scrolling for main holidays
            ItemsListView.ItemAppearing += (sender, e) =>
            {
                var holiday = e.Item as Holiday;

                if (isActive)
                {

                    if (viewModel.Holidays.Last() == holiday)
                    {
                        LoadMoreHolidays();
                    }

                }
            };

        }

        public int page = 0;
        public async void LoadMorePosts()
        {
            page += 1;
            ObservableCollection<Post> morePosts = await Services.GlobalServices.GetPosts(buzz: true, page: page);
            foreach (Post p in morePosts)
            {
                Debug.WriteLine(p.Content);
                Posts.Add(p);
            }
        }

        private int mainHolidaysPage = 0;
        public async void LoadMoreHolidays()
        {
            mainHolidaysPage += 1;
            var moreHolidays = await Services.GlobalServices.GetHolidaysAsync(page: mainHolidaysPage);
            foreach (Holiday h in moreHolidays)
            {
                viewModel.Holidays.Add(h);
            }
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

            if (!item.Active)
            {
                await Application.Current.MainPage.DisplayAlert("Holiday Inactive", "This holiday is currently not ready " +
                        "for viewing. We like to keep a close eye on our holidays " +
                        "and keep them up to date, so sometimes they are removed " +
                        "from the active queue. Try again later!", "OK");
                return;
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

            if (!item.Active)
            {
                await Application.Current.MainPage.DisplayAlert("Holiday Inactive", "This holiday is currently not ready " +
                        "for viewing. We like to keep a close eye on our holidays " +
                        "and keep them up to date, so sometimes they are removed " +
                        "from the active queue. Try again later!", "OK");
                return;
            }

            string holidayId = item.Id;
            if (holidayId != "-1") // Ad
            {
                this.IsEnabled = false;
                await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId, item)));
                this.IsEnabled = true;
            }
        }
        async void LabelSpanToHoliday(object sender, EventArgs args)
        {

            var item = (sender as Label).BindingContext as Post;

            string holidayId = item.HolidayId;
            if (holidayId != "-1") // Ad
            {
                this.IsEnabled = false;
                await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
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
            {
                viewModel.LoadItemsCommand.Execute(null);
            }

            int numRetries = 0;
            while (viewModel.Holidays.Count == 0)
            {
                numRetries++;
                if (numRetries >= 15)
                {
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
                catch (Exception ex)
                {
                    Debug.WriteLine($"Could not sync user data: {ex.Message}");
                }

            }
            //AdBanner.IsVisible = !isPremium;
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
                    await Services.GlobalServices.VoteHoliday(holidayId, "3");

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
                        await Services.GlobalServices.VoteHoliday(holidayId, "1");

                    }
                    else
                    {
                        newVotes -= 2;
                        holiday.CelebrateStatus = "celebrate.png";
                        holiday.Votes = newVotes.ToString();
                        await CelebrateImage.ScaleTo(2, 50);
                        await CelebrateImage.ScaleTo(1, 50);
                        await Services.GlobalServices.VoteHoliday(holidayId, "5");

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
            var holidayLink = App.HolidailyHost + "/holiday?id=" + holiday.Id;
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


        protected void GetBuzzCommand(object sender, EventArgs e)
        {
            page = 0;
            RefreshBuzzFeed();
            SocialMediaList.EndRefresh();
            NoResults.IsVisible = Posts.Count == 0 ? true : false;
        }

        protected async void RefreshHolidaysCommand(object sender, EventArgs e)
        {

            viewModel.LoadItemsCommand.Execute(null);
            ItemsListView.EndRefresh();
            mainHolidaysPage = 0;
        }

        private async void RefreshBuzzFeed()
        {

            try
            {
                //Posts = await Services.GlobalServices.GetBuzzPosts();
                Posts = await Services.GlobalServices.GetPosts(buzz: true);
                SocialMediaList.ItemsSource = Posts;
                NoResults.IsVisible = Posts.Count == 0 ? true : false;
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
                PastHolidaysWrapper.IsVisible = false;
                PastSelected.IsVisible = false;
                TodayLabel.TextColor = Color.FromHex("4c96e8");
                SocialMediaLabel.TextColor = Color.Gray;
                PastLabel.TextColor = Color.Gray;
            }
            else if (searchType == PastLabel.Text)
            {

                HolidayListWrapper.IsVisible = false;
                SocialMediaWrapper.IsVisible = false;
                PastHolidaysWrapper.IsVisible = true;
                TodaySelected.IsVisible = false;
                SocialMediaSelected.IsVisible = false;
                PastSelected.IsVisible = true;
                TodayLabel.TextColor = Color.Gray;
                SocialMediaLabel.TextColor = Color.Gray;
                PastLabel.TextColor = Color.FromHex("4c96e8");

            }
            else
            {
                if (Posts.Count == 0)
                    RefreshBuzzFeed();

                SocialMediaWrapper.IsVisible = true;
                HolidayListWrapper.IsVisible = false;
                PastHolidaysWrapper.IsVisible = false;
                SocialMediaSelected.IsVisible = true;
                TodaySelected.IsVisible = false;
                PastSelected.IsVisible = false;
                TodayLabel.TextColor = Color.Gray;
                SocialMediaLabel.TextColor = Color.FromHex("4c96e8");
                PastLabel.TextColor = Color.Gray;
            }
        }
        //public async void PreviewImage(object sender, EventArgs args)
        //{
        //    try
        //    {
        //        var tweet = (sender as CachedImage).BindingContext as Tweet;
        //        string tweetUrl = tweet.Image;
        //        new PhotoBrowser
        //        {
        //            Photos = new List<Photo>
        //        {
        //            new Photo
        //            {
        //                URL = tweetUrl,
        //                Title = $"{tweet.Handle}'s Photo on Twitter"
        //            }
        //        }
        //        }.Show();
        //    }
        //    catch
        //    {
        //        await DisplayAlert("Ouch!", "Sorry, we couldn't load this" +
        //            " image at the moment", "OK");
        //    }

        //}

        // Post functions
        async void OpenProfile(object sender, EventArgs args)
        {
            dynamic item;
            try
            {
                item = (sender as ContentView).BindingContext as dynamic;
            }
            catch
            {
                item = (sender as ContentView).BindingContext as dynamic;
            }
            if (item.Content == "[deleted]" || item.Content == "[blocked]" || item.Content == "[reported]")
                return;

            string UserName = item.UserName;
            await Navigation.PushAsync(new UserPage(user: null, userName: UserName));
        }
        async void LabelSpanToUser(object sender, EventArgs args)
        {
            dynamic item = (sender as Label).BindingContext as dynamic;
            string UserName = item.UserName;
            await Navigation.PushAsync(new UserPage(user: null, userName: UserName));
        }
        public async void OpenOptions(object sender, EventArgs args)
        {
            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
                return;
            }
            Post post = (sender as ContentView).BindingContext as Post;
            try
            {
                await Navigation.PushPopupAsync(new PostOptionsPopUp(post: post, container: (sender as ContentView)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }

        }
        //public async void ViewPostimage(object sender, EventArgs args)
        //{
        //    Post post = (sender as CachedImage).BindingContext as Post;
        //    try
        //    {
        //        new PhotoBrowser
        //        {
        //            Photos = new List<Photo>
        //        {
        //            new Photo
        //            {
        //                URL = $"{post.Image}",
        //                Title = $"{post.UserName}'s Image"
        //            }
        //        }
        //        }.Show();
        //    }
        //    catch (Exception e)
        //    {
        //        await DisplayAlert("Ouch!", "Sorry, we couldn't load this" +
        //            " image at the moment", "OK");
        //        Debug.WriteLine($"{e}");
        //    }

        //}
        async void Like(object sender, EventArgs args)
        {
            dynamic entity = (sender as StackLayout).BindingContext;
            string ep = entity.GetType() == typeof(Post) ? "posts" : "comments";
            Utils.Vibrate();

            if (!isLoggedIn)
            {
                App.promptLogin(Navigation);
                return;
            }

            if (!entity.LikeEnabled)
                return;
            entity.LikeEnabled = false;
            bool isLiked = entity.LikeImage == "like_neutral.png" ? true : false;


            entity.LikeImage = isLiked == false ? "like_neutral.png" : "like_active.png";
            entity.LikeTextColor = isLiked == false ? Color.FromHex("808080") : Color.FromHex("4c96e8");

            if (isLiked)
            {
                await (sender as StackLayout).ScaleTo(1.5, 50);
                await (sender as StackLayout).ScaleTo(1, 50);
                entity.Likes += 1;

                if (entity.Likes > 1)
                    entity.LikeLabel = "Likes";
                else
                    entity.LikeLabel = "Like";

            }
            else
            {
                entity.Likes -= 1;

                if (entity.Likes > 1)
                    entity.LikeLabel = "Likes";
                else
                    entity.LikeLabel = "Like";

            }

            entity.ShowReactions = entity.Likes > 0 ? true : false;
            // Need to update height
            Utils.RefreshElement((sender as StackLayout));

            try
            {
                var values = new Dictionary<string, string>{
                   { "username", currentUser },
                   { "device_id", devicePushId },
                   { "like", isLiked.ToString() },
                };
                //var content = new FormUrlEncodedContent(values);
                //await App.globalClient.PatchAsync(App.HolidailyHost +
                //    $"/{ep}/" + entity.Id + "/", content);
                await ApiHelpers.MakePatchRequest(values, ep, entity.Id);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not like post: {ex}");
            }
            finally
            {
                await Task.Delay(1000);
                entity.LikeEnabled = true;
            }
        }

    }
}