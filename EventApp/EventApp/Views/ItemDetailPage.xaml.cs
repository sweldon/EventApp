using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using EventApp.Models;
using EventApp.ViewModels;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemDetailPage : ContentPage
    {
        ItemDetailViewModel viewModel;

        public ItemDetailPage(ItemDetailViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = this.viewModel = viewModel;


        }

        public ItemDetailPage()
        {
            InitializeComponent();

            var item = new Item
            {
                Description = "More info on this holiday will be displayed here soon!"
            };

            viewModel = new ItemDetailViewModel(item);
            BindingContext = viewModel;

            
        }
    }
}