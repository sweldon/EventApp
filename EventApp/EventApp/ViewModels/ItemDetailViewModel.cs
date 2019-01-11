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
        public Holiday Holiday { get; set; }
        public ObservableCollection<Comment> Comments { get; set; }
        public Command LoadHolidayComments { get; set; }


        public string currentUser
        {
            get { return Settings.GeneralSettings; }
            set
            {
                if (Settings.GeneralSettings == value)
                    return;
                Settings.GeneralSettings = value;
                OnPropertyChanged();
            }
        }

        public ItemDetailViewModel(Holiday holiday)
        {
            
            Holiday = holiday;
            Title = holiday.Name;
            Comments = new ObservableCollection<Comment>();
            LoadHolidayComments = new Command(async () => await ExecuteLoadCommentsCommand());


            MessagingCenter.Subscribe<NewItemPage>(this, "UpdateComments", (sender) => {
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
                var comments = await CommentStore.GetHolidayCommentsAsync(true, Holiday.Id);
                foreach (var comment in comments)
                {
                    Debug.WriteLine(comment.Content);
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
