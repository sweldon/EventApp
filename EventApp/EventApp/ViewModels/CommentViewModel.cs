using System;
using EventApp.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using EventApp.Views;


namespace EventApp.ViewModels
{
    public class CommentViewModel : BaseViewModel
    {
        public Comment Comment { get; set; }
        public string HolidayId { get; set; }
        public string CommentId { get; set; }
        //public string Content { get; set; }
        //public string UserName { get; set; }
        //public string TimeSince { get; set; }
        //public string UserNameMention { get; set; }
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

        public CommentViewModel(string commentId, string holidayId)
        {

            HolidayId = holidayId;
            CommentId = commentId;
            //Content = comment.Content;
            //UserName = comment.UserName;
            //int UserNameLength = UserName.Length;
            //UserNameMention = UserName.PadRight(UserNameLength + 1, ' ');
            //TimeSince = comment.TimeSince;
            //Title = "Mention";

        }

    }
}
