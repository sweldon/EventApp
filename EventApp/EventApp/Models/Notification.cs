using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;


namespace EventApp.Models
{
    public class Notification : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public bool Read { get; set; }
        public string Content { get; set; }
        public string TimeSince { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Icon { get; set; }
        public string HolidayId { get; set; }
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