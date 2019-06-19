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

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HolidaysPage : ContentPage
    {
        HolidaysViewModel viewModel;

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

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {

            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            var item = args.SelectedItem as Holiday;
            if (item.Id != "-1") // Ad
            {
                this.IsEnabled = false;
                await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(item.Id)));
                this.IsEnabled = true;
            }

        }

        async void ImageToHoliday(object sender, EventArgs args)
        {

            var item = (sender as Image).BindingContext as Holiday;
            string holidayId = item.Id;
            if (holidayId != "-1") // Ad
            {
                this.IsEnabled = false;
                await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
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
                await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
                this.IsEnabled = true;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            //if (viewModel.Holidays.Count == 0)
            viewModel.LoadItemsCommand.Execute(null);

            // Manually open menu page on swipe only on main page
            //(Application.Current.MainPage as RootPage).IsGestureEnabled = false;

        }

        async void OnShareTapped(object sender, EventArgs args)
        {
            this.IsEnabled = false;

            var holiday = (sender as Label).BindingContext as Holiday;
            this.IsEnabled = false;

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

        async void OnSharePicTapped(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            var holiday = (sender as Image).BindingContext as Holiday;

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