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
            viewModel.Title = "Find Holidays";
            DateTime SelectedDate = viewModel.SelectedDate;
            TopHolidayList.ItemSelected += OnPopularSelected;
        }

        public void ToggleFindType(object sender, EventArgs args)
        {

            var contentViewSender = (ContentView)sender;
            var labelSender = (Label)contentViewSender.Children[0];
            var searchType = labelSender.Text;
            if (searchType == "Search Holidays")
            {
                SearchWrapper.IsVisible = true;
                PopularWrapper.IsVisible = false;
                SearchSelected.IsVisible = true;
                PopularSelected.IsVisible = false;
                SearchHolidays.TextColor = Color.FromHex("4c96e8");
                PopularHolidays.TextColor = Color.Gray;
            }
            else
            {
                if (viewModel.TopHolidays.Count == 0)
                    viewModel.LoadTopHolidays.Execute(null);
                PopularWrapper.IsVisible = true;
                SearchWrapper.IsVisible = false;
                PopularSelected.IsVisible = true;
                SearchSelected.IsVisible = false;
                SearchHolidays.TextColor = Color.Gray;
                PopularHolidays.TextColor = Color.FromHex("4c96e8");
            }
        }


        async void OnPopularSelected(object sender, SelectedItemChangedEventArgs args)
        {
            this.IsEnabled = false;
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            var item = args.SelectedItem as Holiday;
            await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(item.Id, item)));
            this.IsEnabled = true;
        }

        public async void SearchByDate(object sender, DateChangedEventArgs args)
        {
            
            SearchPlaceholder.IsVisible = false;
            NoResultsBox.IsVisible = false;
            var searchedDate = DateValue.Date;
            var dateStr = searchedDate.ToString();
            try
            {
                IEnumerable<Holiday> SearchResults = await viewModel.HolidayStore.SearchHolidays(dateStr);
                SearchHolidayList.ItemsSource = SearchResults;
                NoResultsBox.IsVisible = SearchResults.Count() > 0 ? false : true;
                SearchHolidayList.BackgroundColor = SearchResults.Count() > 0 ? Color.FromHex("E0E0E0") : Color.FromHex("FFFFFF");
            }
            catch
            {
                await DisplayAlert("Error", "Couldn't connect to Holidaily", "OK");
            }

        }

        public async void DaySearched(object sender, EventArgs e)
        {
            
            SearchPlaceholder.IsVisible = false;
            try
            {
                if (!string.IsNullOrEmpty(SearchValue.Text))
                {
                    var searchText = SearchValue.Text;

                    if (searchText.Length > 2)
                    {
                        // Do some searching
                        IEnumerable<Holiday> SearchResults = await viewModel.HolidayStore.SearchHolidays(searchText);
                        SearchHolidayList.ItemsSource = SearchResults;
                        NoResultsBox.IsVisible = SearchResults.Count() > 0 ? false : true;
                        SearchHolidayList.BackgroundColor = SearchResults.Count() > 0 ? Color.FromHex("E0E0E0") : Color.FromHex("FFFFFF");

                    }
                    else
                    {
                        await DisplayAlert("Need more!", "Give us more to work with", "I'll type more");
                    }
                }
                else
                {
                    await DisplayAlert("Need more!", "We can't give you anything if you don't give us anything", "Obviously");
                }
            }
            catch
            {
                await DisplayAlert("Huh", "That's a strange search, please try again", "OK");
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

        async void OnItemSelected(object sender,
            SelectedItemChangedEventArgs args)
        {
            this.IsEnabled = false;
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            var item = args.SelectedItem as Holiday;
            if (!item.Active)
            {
                await DisplayAlert("Holiday Inactive", "This holiday is currently not ready " +
                        "for viewing. We like to keep a close eye on our holidays " +
                        "and keep them up to date, so sometimes they are removed " +
                        "from the active queue. Try again later!", "OK");
            }
            else
            {
                await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(item.Id, item)));
            }
            this.IsEnabled = true;
        }

        async void ImageToHoliday(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            var item = (sender as ContentView).BindingContext as Holiday;
            if (!item.Active)
            {
                await DisplayAlert("Holiday Inactive", "This holiday is currently not ready " +
                        "for viewing. We like to keep a close eye on our holidays " +
                        "and keep them up to date, so sometimes they are removed " +
                        "from the active queue. Try again later!", "OK");
            }
            else
            {
                string holidayId = item.Id;
                await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId, item)));
            }
            this.IsEnabled = true;
        }

        async void LabelToHoliday(object sender, EventArgs args)
        {
            this.IsEnabled = false;
            var item = (sender as Label).BindingContext as Holiday;
            if (!item.Active)
            {
                await DisplayAlert("Holiday Inactive", "This holiday is currently not ready " +
                        "for viewing. We like to keep a close eye on our holidays " +
                        "and keep them up to date, so sometimes they are removed " +
                        "from the active queue. Try again later!", "OK");
            }
            else
            {
                string holidayId = item.Id;

                await Navigation.PushAsync(new HolidayDetailPage(new
                    HolidayDetailViewModel(holidayId, item)));
            }
            this.IsEnabled = true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            //MessagingCenter.Send(this, "UpdateToolbar", true);
        }

        public void Share(object sender, EventArgs args)
        {
            // TODO Consider making the share function a global util function.
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
            var holidayLink = "https://holidailyapp.com/holiday?id=" + holiday.Id;
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