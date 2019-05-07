using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventApp.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EventApp.Models;
using System.Diagnostics;
namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Trending : ContentPage
    {
        TrendingViewModel viewModel;

        public Trending()
        {
            InitializeComponent();
            BindingContext = viewModel = new TrendingViewModel(); ;
            Title = "Trending Holidays";

            TopHolidayList.ItemSelected += OnItemSelected;
        }


        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            this.IsEnabled = false;
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }
            var item = args.SelectedItem as Holiday;
            await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(item.Id)));
            this.IsEnabled = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.TopHolidays.Count == 0)
                viewModel.LoadTopHolidays.Execute(null);


        }

    }
}