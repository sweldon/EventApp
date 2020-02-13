using System;
using EventApp.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using EventApp.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EventApp.ViewModels
{
    public class HolidayDetailViewModel : BaseViewModel
    {
        public bool isLoading;
        public int page = 0;
        public bool allCommentsLoaded = false;
        public Holiday Holiday { get; set; }
        private ObservableCollection<CommentList> CommentList;
        public ObservableCollection<CommentList> GroupedCommentList { get { return CommentList; }
            set { CommentList = value; base.OnPropertyChanged(); } }
        public Command LoadHolidayComments { get; set; }
        public Command GetMoreComments { get; set; }
        public ObservableCollection<CommentList> moreComments;
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

        public HolidayDetailViewModel(string holidayId, Holiday holidayObject)
        {

            // Coming in from a "Share" link
            Holiday = holidayObject;
            HolidayId = holidayId;
            GroupedCommentList = new ObservableCollection<CommentList>();
            LoadHolidayComments = new Command(async () => await ExecuteLoadCommentsCommand());

            GetMoreComments = new Command(async () => await LoadMoreComments());

            MessagingCenter.Subscribe<NewCommentPage>(this, "UpdateComments", (sender) => {
                ExecuteLoadCommentsCommand();
            });

            MessagingCenter.Subscribe<CommentPage>(this, "UpdateComments", (sender) => {
                ExecuteLoadCommentsCommand();
            });

            MessagingCenter.Subscribe<HolidayDetailPage>(this, "UpdateComments", (sender) => {
                ExecuteLoadCommentsCommand();

            });



        }

        private async Task LoadMoreComments()
        {
            isLoading = true;
            try
            {
                if (!allCommentsLoaded) {
                    moreComments = await CommentStore.GetMoreComments(HolidayId, currentUser, page.ToString());

                    if(moreComments.Count == 0)
                    {
                        allCommentsLoaded = true;
                    }
                    else
                    {
                        foreach(var thread in moreComments)
                        {
                            GroupedCommentList.Add(thread);
                        }
                        
                        page += 1;
                    }
                }



            }catch{
                Debug.WriteLine("Throttled");
            }
            isLoading = false;
        }

        async Task ExecuteLoadCommentsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            page = 0;
            allCommentsLoaded = false;
            try
            {
                GroupedCommentList = new ObservableCollection<CommentList>();
                var allComments = new ObservableCollection<CommentList>();
                var threads = await CommentStore.GetHolidayCommentsAsync(
                    true, HolidayId, currentUser);

                foreach (var group in threads)
                {
                    var commentList = new CommentList();
                    foreach (var comment in group)
                    {
                        commentList.Add(comment);
                    }
                    allComments.Insert(0, commentList);
                }

                GroupedCommentList = allComments;


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
