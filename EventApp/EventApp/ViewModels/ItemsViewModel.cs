using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using EventApp.Models;
using EventApp.Views;

namespace EventApp.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        public ObservableCollection<Holiday> Items { get; set; }
        public Command LoadItemsCommand { get; set; }

        public ItemsViewModel()
        {
            Items = new ObservableCollection<Holiday>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            //MessagingCenter.Subscribe<NewItemPage, User>(this, "AddItem", async (obj, item) =>
            //{
            //    var newItem = item as User;
            //    Items.Add(newItem);
            //    await DataStore.AddItemAsync(newItem);
            //});

        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Items.Clear();
                var holidays = await DataStore.GetItemsAsync(true);
                foreach (var holiday in holidays)
                {
                    Items.Insert(0, holiday);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}