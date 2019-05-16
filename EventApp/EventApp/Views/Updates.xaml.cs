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
    public partial class Updates : ContentPage
    {
        UpdateViewModel viewModel;

        public Updates()
        {
            InitializeComponent();
            BindingContext = viewModel = new UpdateViewModel(); ;
            Title = "Holidaily News";
            UpdateList.ItemSelected += Selected;
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            //if (viewModel.Notifications.Count == 0)
            viewModel.LoadUpdates.Execute(null);


        }

        async void Selected(object sender, SelectedItemChangedEventArgs args)
        {
            ((ListView)sender).SelectedItem = null;
            if (args.SelectedItem == null)
            {
                return;
            }

        }

    }
}