using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace EventApp.Models
{
    public class Comment : INotifyPropertyChanged
    {

        public string Id { get; set; }
        private string content;
        public string Content
        {
            get { return content; }
            set
            {
                if (content == value)
                {
                    return;
                }
                content = value;
                OnPropertyChanged();
            }
        }

        private string username;
        public string UserName
        {
            get { return username; }
            set
            {
                if (username == value)
                {
                    return;
                }
                username = value;
                OnPropertyChanged();
            }
        }

        private string showreply;
        public string ShowReply
        {
            get { return showreply; }
            set
            {
                if (showreply == value)
                {
                    return;
                }
                showreply = value;
                OnPropertyChanged();
            }
        }

        private string showdelete;
        public string ShowDelete
        {
            get { return showdelete; }
            set
            {
                if (showdelete == value)
                {
                    return;
                }
                showdelete = value;
                OnPropertyChanged();
            }
        }


        public string HolidayId { get; set; }
        public string TimeSince { get; set; }
        public Thickness ThreadPadding { get; set; }

        public string Parent { get; set; }

        private string upvote;
        public string UpVoteStatus
        {
            get { return upvote; }
            set
            {
                if (upvote == value)
                {
                    return;
                }
                upvote = value;
                OnPropertyChanged();
            }
        }

        private string downvote;
        public string DownVoteStatus
        {
            get { return downvote; }
            set
            {
                if (downvote == value)
                {
                    return;
                }
                downvote = value;
                OnPropertyChanged();
            }
        }


        private int votes;
        public int Votes
        {
            get { return votes; }
            set
            {
                if (votes == value)
                {
                    return;
                }
                votes = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion



    }


}