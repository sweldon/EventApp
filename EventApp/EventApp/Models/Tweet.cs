using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace EventApp.Models
{
    // TODO consider making this a universal "post" object? then
    // have an attibute be the logo, i.e. Twitter bird, fb logo, etc
    public class Tweet : INotifyPropertyChanged
    {
        public string UserName { get; set; }
        public string Body { get; set; }
        public string Timestamp { get; set; }
        public string Image { get; set; }
        public string UserProfileImage { get; set; }
        public bool ShowImage { get; set; }
        public string Handle { get; set; }
        public bool Verified { get; set; }
        public string Url { get; set; }

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