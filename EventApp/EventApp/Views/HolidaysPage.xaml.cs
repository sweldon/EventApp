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
            //DayNumberLabel.Text = dayNumber;

            List<string> months = new List<string>() { 
                "January","February","March","April","May","June","July",
                "August", "September", "October", "November", "December"
            };

            string monthString = months[monthNumber - 1];
            string todayString = currentDate.DayOfWeek.ToString();
            //MonthLabel.Text = monthString;
            ItemsListView.ItemSelected += OnItemSelected;
            //TodayLabel.Text = todayString + ", " + monthString + " " + dayNumber;

            viewModel.Title = todayString + ", " + monthString + " " + dayNumber;

            //swipeContainer.Swipe += (sender, e) =>
            //{
            //    switch (e.Direction)
            //    {
            //        case SwipeDirection.Right:
            //            (Application.Current.MainPage as MasterDetailPage).IsPresented = true;
            //            break;
            //    }
            //};


        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            var item = args.SelectedItem as Holiday;
            if(item.Id != "-1") // Ad
            {
                this.IsEnabled = false;
                await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(item.Id)));
                this.IsEnabled = true;
            }

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Holidays.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);

            // Manually open menu page on swipe only on main page
            //(Application.Current.MainPage as RootPage).IsGestureEnabled = false;

        }
    }
}