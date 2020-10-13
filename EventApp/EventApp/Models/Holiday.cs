using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace EventApp.Models
{
    public class Holiday : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Blurb { get; set; }
        public int NumComments { get; set; }
        public string TimeSince { get; set; }
        public string DescriptionShort { get; set; }
        public string HolidayImage { get; set; }
        public bool ShowHolidayContent { get; set; }
        public string Date { get; set; }
        public bool Active { get; set; }

        public bool showad;
        public bool ShowAd
        {
            get { return showad; }
            set
            {
                if (showad == value)
                {
                    return;
                }
                showad = value;
                OnPropertyChanged();
            }
        }

        public string votes;
        public string Votes
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

        public string celebrating;
        public string CelebrateStatus
        {
            get { return celebrating; }
            set
            {
                if (celebrating == value)
                {
                    return;
                }
                celebrating = value;
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