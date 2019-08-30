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
#if __IOS__
using UIKit;
#endif

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HolidaysPage : ContentPage
    {
        HolidaysViewModel viewModel;

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

        public HolidaysPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new HolidaysViewModel();
            DateTime currentDate = DateTime.Today;

            string dateString = currentDate.ToString("dd-MM-yyyy");
            string dayNumber = dateString.Split('-')[0].TrimStart('0');
            int monthNumber = Int32.Parse(dateString.Split('-')[1]);

            List<string> months = new List<string>() {
                "January","February","March","April","May","June","July",
                "August", "September", "October", "November", "December"
            };

            string monthString = months[monthNumber - 1];
            string todayString = currentDate.DayOfWeek.ToString();
            //ItemsListView.ItemSelected += OnItemSelected;
            ItemsListView.ItemTapped += (object sender, ItemTappedEventArgs e) => {
                if (e.Item == null) return;
                ((ListView)sender).SelectedItem = null;
            };
            //TodayLabel.Text = todayString + ", " + monthString + " " + dayNumber;
            viewModel.Title = todayString + ", " + monthString + " " + dayNumber;


        }

        async void ImageToHoliday(object sender, SelectedItemChangedEventArgs args)
        {
         

            var item = (sender as Image).BindingContext as Holiday;
            var index = viewModel.Holidays.IndexOf(item);
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
            var index = viewModel.Holidays.IndexOf(item);
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

            if (viewModel.Holidays.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);

        }


        async void OnCelebrateTapped(object sender, EventArgs args)
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

            this.IsEnabled = false;
            
            var view = (VisualElement)sender;
            var GridObject = (Grid)view.Parent;
            var CelebrateImage = (Image)GridObject.Children[4];
            var UpVoteImageFile = CelebrateImage.Source as FileImageSource;
            var UpVoteIcon = UpVoteImageFile.File;
            
            var holidayId = "none";
            var votesInt = -1;
            var CelebrateLabel = (Label)GridObject.Children[5];
            bool isLabel = false;
            var holiday = new Holiday();
            try
            {
                holiday = (sender as Label).BindingContext as Holiday;
                votesInt = Int32.Parse((sender as Label).Text.Split(null)[0]);
                holidayId = holiday.Id;
                isLabel = true;
            }
            catch
            {
                holiday = (sender as Image).BindingContext as Holiday;
                votesInt = Int32.Parse(CelebrateLabel.Text.Split(null)[0]);
                holidayId = holiday.Id;
            }

            var newVotes = votesInt;

            if (isLoggedIn == "no")
            {
                this.IsEnabled = false;
                await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
                this.IsEnabled = true;
            }
            else
            {
                if (UpVoteIcon == "celebrate_active.png")
                {
                    // Undo

                    newVotes -= 1;
                    //if (isLabel)
                    //{
                    //    (sender as Label).Text = newVotes.ToString() + " Celebrating!";
                    //}
                    //else
                    //{
                    //    CelebrateLabel.Text = newVotes.ToString() + " Celebrating!";
                    //}
                    
                    holiday.CelebrateStatus = "celebrate.png";
                    holiday.Votes = newVotes.ToString();
                    await CelebrateImage.ScaleTo(2, 50);
                    await CelebrateImage.ScaleTo(1, 50);
                    await viewModel.HolidayStore.VoteHoliday(holidayId, currentUser, "3");

                }
                else
                {
                    // Only allow if user hasnt already downvoted
                    newVotes += 1;
                    if (newVotes <= votesInt + 1 && newVotes >= votesInt - 1)
                    {
                        //if (isLabel)
                        //{
                        //    (sender as Label).Text = newVotes.ToString() + " Celebrating!";
                        //}
                        //else
                        //{
                        //    CelebrateLabel.Text = newVotes.ToString() + " Celebrating!";
                        //}
                        holiday.CelebrateStatus = "celebrate_active.png";
                        holiday.Votes = newVotes.ToString();
                        await CelebrateImage.ScaleTo(2, 50);
                        await CelebrateImage.ScaleTo(1, 50);
                        await viewModel.HolidayStore.VoteHoliday(holidayId, currentUser, "1");

                    }
                    else
                    {
                        newVotes -= 2;
                        //if (isLabel)
                        //{
                        //    (sender as Label).Text = newVotes.ToString() + " Celebrating!";
                        //}
                        //else
                        //{
                        //    CelebrateLabel.Text = newVotes.ToString() + " Celebrating!";
                        //}
                        holiday.CelebrateStatus = "celebrate.png";
                        holiday.Votes = newVotes.ToString();
                        await CelebrateImage.ScaleTo(2, 50);
                        await CelebrateImage.ScaleTo(1, 50);
                        await viewModel.HolidayStore.VoteHoliday(holidayId, currentUser, "5");

                    }

                }
            }

            this.IsEnabled = true;

        }

        async void Share(object sender, EventArgs args)
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
            var timeSince = holiday.Date;
            string HolidayDescriptionShort = holiday.Description.Length <= 90 ? holiday.Description + "\nSee more! https://holidailyapp.com/holiday?id=" + holiday.Id : holiday.Description.Substring(0, 90) + "...\nSee more! https://holidailyapp.com/holiday?id=" + holiday.Id;
            this.IsEnabled = false;
            string action = await DisplayActionSheet("How would you like to share?", "Cancel", null, "Text Message");
            if (action == "Text Message")
            {
                try
                {
                    var messageContents = holidayName + "! (" + timeSince + ") " + HolidayDescriptionShort;
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

    }
}