using System;
using EventApp.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using EventApp.Views;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace EventApp.ViewModels
{
    public class HolidayDetailViewModel : BaseViewModel
    {
        public Holiday Holiday { get; set; } // Set by async GetHolidayById call when page loads
        public ObservableCollection<Comment> Comments { get; set; }
        public Command LoadHolidayComments { get; set; }
        public string HolidayId { get; set; }

        public string currentUser
        {
            get { return Settings.CurrentUser; }
            set
            {
                if (Settings.CurrentUser == value)
                    return;
                Settings.CurrentUser = value;
                OnPropertyChanged();
            }
        }

        public HolidayDetailViewModel(string holidayId)
        {

            HolidayId = holidayId;
            
            Comments = new ObservableCollection<Comment>();
            LoadHolidayComments = new Command(async () => await ExecuteLoadCommentsCommand());

            MessagingCenter.Subscribe<NewCommentPage>(this, "UpdateComments", (sender) => {
                ExecuteLoadCommentsCommand();
            });

            MessagingCenter.Subscribe<CommentPage>(this, "UpdateComments", (sender) => {
                ExecuteLoadCommentsCommand();
            });


        }

        async Task ExecuteLoadCommentsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Comments.Clear();
                var comments = await CommentStore.GetHolidayCommentsAsync(true, HolidayId);
                foreach (var comment in comments)
                {
                    Comments.Insert(0, comment);
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
