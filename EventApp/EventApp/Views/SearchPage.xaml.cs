﻿using System;
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
    public partial class SearchPage : ContentPage
    {
        SearchViewModel viewModel;

        public SearchPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new SearchViewModel();

            SearchHolidayList.ItemTapped += (object sender, ItemTappedEventArgs e) => {
                if (e.Item == null) return;
                ((ListView)sender).SelectedItem = null;
            };

            viewModel.Title = "Search a Holiday";

            DateTime SelectedDate = viewModel.SelectedDate;



        }

        async void ToggleSearchType(object sender, EventArgs args)
        {

            var labelSender = (Label)sender;
            var searchType = labelSender.Text;
            if(searchType=="Search by Name")
            {
                NameSearch.IsVisible = true;
                DateSearch.IsVisible = false;
                SearchByNameText.TextColor = Color.FromHex("4c96e8");
                SearchByDateText.TextColor = Color.Gray;
                SearchValue.Focus();
            }
            else
            {
                NameSearch.IsVisible = false;
                DateSearch.IsVisible = true;
                SearchByNameText.TextColor = Color.Gray;
                SearchByDateText.TextColor = Color.FromHex("4c96e8");
                DateValue.Focus();
                #if __ANDROID__
                DependencyService.Get<IKeyboardHelper>().HideKeyboard();
                #endif
            }


        }

        public async void SearchByDate(object sender, DateChangedEventArgs args)
        {
            var searchedDate = DateValue.Date;
            var dateStr = searchedDate.ToString();
            SearchHolidayList.ItemsSource = await viewModel.HolidayStore.SearchHolidays(dateStr);

        }

        public async void DaySearched(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(SearchValue.Text))
                {
                    var searchText = SearchValue.Text;

                    if (searchText.Length > 2)
                    {
                        // Do some searching
                        SearchHolidayList.ItemsSource = await viewModel.HolidayStore.SearchHolidays(searchText);
                    }
                    else
                    {
                        DisplayAlert("Need more!", "Give us more to work with", "I'll type more");
                    }
                }
                else
                {
                    DisplayAlert("Need more!", "We can't give you anything if you don't give us anything", "Obviously");
                }
            }
            catch (Exception ex)
            {
                // They searched something w/ weird chars, like emojis
            }

        
        }

        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if(SearchValue.Text.Length > 3)
            {
                DisplayAlert("Search!",
                    "Do some searching", "Yes", "No");
            }
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if(SearchByDateText.TextColor == Color.Gray)
            {
                await Task.Delay(100);
                SearchValue.Focus();
            }



        }

        async void OnShareTapped(object sender, EventArgs args)
        {
            this.IsEnabled = false;

            var holiday = (sender as Label).BindingContext as Holiday;
            this.IsEnabled = false;

            var holidayName = holiday.Name;
            var holidayDate = holiday.Date;
            string HolidayDescriptionShort = holiday.Description.Length <= 90 ? holiday.Description + "\nSee more! https://holidailyapp.com/holiday?id=" + holiday.Id : holiday.Description.Substring(0, 90) + "...\nSee more! https://holidailyapp.com/holiday?id=" + holiday.Id;
            this.IsEnabled = false;
            string action = await DisplayActionSheet("How would you like to share?", "Cancel", null, "Text Message");
            if (action == "Text Message")
            {
                try
                {
                    var messageContents = holidayName + "! (" + holidayDate + ") " + HolidayDescriptionShort;
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
            var holidayDate = holiday.Date;
            string HolidayDescriptionShort = holiday.Description.Length <= 90 ? holiday.Description + "\nSee more! https://holidailyapp.com/holiday?id=" + holiday.Id : holiday.Description.Substring(0, 90) + "...\nSee more! https://holidailyapp.com/holiday?id=" + holiday.Id;
            this.IsEnabled = false;
            string action = await DisplayActionSheet("How would you like to share?", "Cancel", null, "Text Message");
            if (action == "Text Message")
            {
                try
                {
                    var messageContents = holidayName + "! (" + holidayDate + ") " + HolidayDescriptionShort;
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