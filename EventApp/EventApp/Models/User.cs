using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EventApp.Models
{
    public class User : INotifyPropertyChanged
    {
        public string UserName { get; set; }
        public string Confetti { get; set; }
        public string Submissions { get; set; }
        public string Approved { get; set; }
        public string Comments { get; set; }
        [DefaultValue(false)]
        public bool Premium { get; set; }
        private string avatar;
        public string LastOnline { get; set; }
        public string Email { get; set; }
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