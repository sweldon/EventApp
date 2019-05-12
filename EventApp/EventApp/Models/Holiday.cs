using System;

namespace EventApp.Models
{
    public class Holiday
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Votes { get; set; }
        public int NumComments { get; set; }
        public string TimeSince { get; set; }
        public string DescriptionShort { get; set; }
        public string HolidayImage { get; set; }
        public bool ShowHolidayContent { get; set; }
        public bool ShowAd { get; set; }
    }
}