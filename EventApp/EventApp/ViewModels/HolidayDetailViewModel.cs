
using EventApp.Models;


namespace EventApp.ViewModels
{
    public class HolidayDetailViewModel : BaseViewModel
    {
        public Holiday Holiday { get; set; }
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

        public HolidayDetailViewModel(string holidayId,
            Holiday holidayObject = null
            )
        {

            // Coming in from a "Share" link
            Holiday = holidayObject;
            HolidayId = holidayId;

        }

    }
}
