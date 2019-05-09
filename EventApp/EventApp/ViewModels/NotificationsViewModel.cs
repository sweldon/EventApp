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
    public class NotificationsViewModel : BaseViewModel
    {
        public Command LoadNotifications { get; set; }
        public ObservableCollection<Notification> Notifications { get; set; }

        public NotificationsViewModel()
        {
            Notifications = new ObservableCollection<Notification>();
            LoadNotifications = new Command(async () => await ExecuteLoadNotifications());

            MessagingCenter.Subscribe<NewCommentPage>(this, "UpdateNotifications", (sender) => {
                ExecuteLoadNotifications();
            });
        }

        async Task ExecuteLoadNotifications()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Notifications.Clear();
                var notifications = await NotificationStore.GetUserNotifications(true);
                foreach (var n in notifications)
                {
                    Notifications.Insert(0, n);
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
