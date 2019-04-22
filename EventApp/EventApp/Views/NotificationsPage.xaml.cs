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
    public partial class NotificationsPage : ContentPage
    {
        NotificationsViewModel viewModel;

        public NotificationsPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new NotificationsViewModel(); ;
            Title = "Your Recent Activity";

            //TopHolidayList.ItemSelected += OnItemSelected;
        }


        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            //((ListView)sender).SelectedItem = null;
            //if (args.SelectedItem == null)
            //{
            //    return;
            //}
            //var item = args.SelectedItem as Holiday;
            //await Navigation.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(item.Id)));

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            //if (viewModel.TopHolidays.Count == 0)
            //    viewModel.LoadTopHolidays.Execute(null);


        }

    }
}