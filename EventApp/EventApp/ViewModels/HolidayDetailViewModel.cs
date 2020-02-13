using System;
using EventApp.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using EventApp.Views;
using System.Collections.Generic;

namespace EventApp.ViewModels
{
    public class HolidayDetailViewModel : BaseViewModel
    {
        //public bool isLoading;
        public Holiday Holiday { get; set; }
        private List<CommentList> CommentList;
        public List<CommentList> GroupedCommentList { get { return CommentList; }
            set { CommentList = value; base.OnPropertyChanged(); } }
        public Command LoadHolidayComments { get; set; }
        //public Command GetMoreComments { get; set; }
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
            bool isLoading = false;
            GroupedCommentList = new List<CommentList>();
            LoadHolidayComments = new Command(async () => await ExecuteLoadCommentsCommand());
            //GetMoreComments = new Command(async () => await LoadMoreComments());

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

        //async Task LoadMoreComments()
        //{
        //   isLoading = true;
        //   List<CommentList> test = new List<CommentList>();
        //    var testGroup = new CommentList();
        //    testGroup.Add(new Comment()
        //    {
        //        Id = "999",
        //        Content = "",
        //        HolidayId = "1",
        //        UserName = "test",
        //        TimeSince = "test",
        //        ShowReply = "test",
        //        ShowDelete = "test",
        //        Votes = 0,
        //        UpVoteStatus = "up.png",
        //        DownVoteStatus = "down",
        //        Parent = "test",
        //        ThreadPadding = new Thickness(Convert.ToDouble("10"), 10, 10, 10),
        //    });
        //    test.Add(testGroup);
        //    GroupedCommentList = test;
        //    isLoading = false;
        //    //Debug.WriteLine("List size " + GroupedCommentList.Count);
        //}

        async Task ExecuteLoadCommentsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                GroupedCommentList = new List<CommentList>();
                var allComments = new List<CommentList>();
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
