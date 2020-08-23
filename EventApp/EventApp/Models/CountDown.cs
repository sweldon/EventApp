using System;
namespace EventApp.Models
{
    public class CountDown
    {
        public DateTime Date { get; set; }
        //public string EventTitle { get; set; }
        public TimeSpan Timespan { get; set; }
        public string Days => Timespan.Days.ToString("00");
        public string Hours => Timespan.Hours.ToString("00");
        public string Minutes => Timespan.Minutes.ToString("00");
        public string Seconds => Timespan.Seconds.ToString("00");
        //public string BgColor { get; set; }
    }
}
