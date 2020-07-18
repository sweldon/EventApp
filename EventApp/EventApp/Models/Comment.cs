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

        private bool showReport;
        [DefaultValue(true)]
        public bool ShowReport
        {
            get { return showReport; }
            set
            {
                if (showReport == value)
                {
                    return;
                }
                showReport = value;
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

        private string showedit;
        public string ShowEdit
        {
            get { return showedit; }
            set
            {
                if (showedit == value)
                {
                    return;
                }
                showedit = value;
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

        private double opacity;
        [DefaultValue(1)]
        public double ElementOpacity
        {
            get { return opacity; }
            set
            {
                if (opacity == value)
                {
                    return;
                }
                opacity = value;
                OnPropertyChanged();
            }
        }

        private bool enabled;
        [DefaultValue(true)]
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled == value)
                {
                    return;
                }
                enabled = value;
                OnPropertyChanged();
            }
        }
        private string avatar;
        public string Avatar
        {
            get { return avatar; }
            set
            {
                if (avatar == value)
                {
                    return;
                }
                avatar = value;
                OnPropertyChanged();
            }
        }
        public string HolidayId { get; set; }
        public string TimeSince { get; set; }
        private string timeSinceEdit;
        public string TimeSinceEdit
        {
            get { return timeSinceEdit; }
            set
            {
                if (timeSinceEdit == value)
                {
                    return;
                }
                timeSinceEdit = value;
                OnPropertyChanged();
            }
        }
        public Thickness ThreadPadding { get; set; }
        public string Parent { get; set; }

        private Color bg;
        public Color BackgroundColor
        {
            get { return bg; }
            set
            {
                if (bg == value)
                {
                    return;
                }
                bg = value;
                OnPropertyChanged();
            }
        }


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