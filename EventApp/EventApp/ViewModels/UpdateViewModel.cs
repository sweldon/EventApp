using System;
using EventApp.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using EventApp.Views;
using System.Collections.Generic;

namespace EventApp.ViewModels
{
    public class UpdateViewModel : BaseViewModel
    {
        public Command LoadUpdates { get; set; }
        public ObservableCollection<Notification> Updates { get; set; }

        public UpdateViewModel()
        {
            Updates = new ObservableCollection<Notification>();
            LoadUpdates = new Command(async () => await ExecuteLoadUpdates());

        }

        async Task ExecuteLoadUpdates()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Updates.Clear();
                var updates = await NotificationStore.GetUpdates(true);
                foreach (var n in updates)
                {
                    Updates.Insert(0, n);
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
