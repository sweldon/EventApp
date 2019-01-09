using System;
using EventApp.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using EventApp.Views;


namespace EventApp.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public Holiday Item { get; set; }
        public Command LoadItemsCommand { get; set; }

        public ItemDetailViewModel(Holiday item)
        {
            
            Item = item;
            Title = item.Name;

        }

    }
}
