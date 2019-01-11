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
    public partial class ItemsPage : ContentPage
    {
        ItemsViewModel viewModel;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new ItemsViewModel();
            DateTime currentDate = DateTime.Today;
            string dateString = currentDate.ToString("dd-MM-yyyy");
            string dayNumber = dateString.Split('-')[0].TrimStart('0');
            int monthNumber = Int32.Parse(dateString.Split('-')[1]);
            DayNumberLabel.Text = dayNumber;

            Debug.WriteLine(dayNumber);
            Debug.WriteLine(monthNumber);

            List<string> months = new List<string>() { 
                "January","February","March","April","May","June","July",
                "August", "September", "October", "November", "December"
            };

            string monthString = months[monthNumber - 1];
            MonthLabel.Text = monthString;
            ItemsListView.ItemSelected += OnItemSelected;


        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {

            if (args.SelectedItem == null)
            {
                return; // ensures we ignore this handler when the selection is just being cleared
            }
            var item = args.SelectedItem as Holiday;
            await Navigation.PushAsync(new ItemDetailPage(new ItemDetailViewModel(item)));
            ((ListView)sender).SelectedItem = null; // clears the 'selected' background


        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
            //await Navigation.PushModalAsync(new NavigationPage(new NewItemPage()));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }
    }
}