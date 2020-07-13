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

        public string showAds;
        public HolidaysPage()
        {
            InitializeComponent();
            //NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = viewModel = new HolidaysViewModel();

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



        protected override async void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Send(this, "UpdateToolbar", true);
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
                await LoadingIcon.ScaleTo(15, 100);
                await LoadingIcon.ScaleTo(10, 100);
                if (numRetries >= 15) {
                    await DisplayAlert("Error", "Couldn't connect to Holidaily", "OK");
                    return;
                }
                await Task.Delay(2000);
            }
            AdBanner.IsVisible = !isPremium;
            if (OpenNotifications)
            {
                await Navigation.PushModalAsync(new NavigationPage(new NotificationsPage()));
                OpenNotifications = false;
            }

            if (App.syncDeviceToken)
            {
                Utils.syncUser();
            }

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

    }
}