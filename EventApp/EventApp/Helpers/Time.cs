

using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace EventApp
{

    public static class Time
    {

        const int SECOND = 1;
        const int MINUTE = 60 * SECOND;
        const int HOUR = 60 * MINUTE;
        const int DAY = 24 * HOUR;
        const int MONTH = 30 * DAY;
        public static int ActiveHoliday(string date)
        {
            string year =  DateTime.Now.Year.ToString();
            string[] parts;
            Dictionary<string, string> stringToNumericMonth = new Dictionary<string, string>()
                                            {
                                                {"Jan","01"},
                                                {"Feb", "02"},
                                                {"Mar","03"},
                                                {"Apr","04"},
                                                {"May","05"},
                                                {"Jun","06"},
                                                {"Jul","07"},
                                                {"Aug","08"},
                                                {"Sep","09"},
                                                {"Oct","10"},
                                                {"Nov","11"},
                                                {"Dec","12"},
                                            };
            try
            {
                parts = date.Split(". ");
                string month = parts[0];
                string day = parts[1];
                string dateString = year + "-" + stringToNumericMonth[month] + "-" + day.PadLeft(2, '0') + " 00:00:00";
                var timeStampDatetime = DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                string currentTimeZone = TimeZoneInfo.Local.StandardName;
                TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                DateTime thisTime = DateTime.Now;
                bool isDaylight = TimeZoneInfo.Local.IsDaylightSavingTime(thisTime);
                DateTime timeStamp = TimeZoneInfo.ConvertTimeFromUtc(timeStampDatetime, localTimeZone);
                if (isDaylight)
                    timeStamp = timeStamp.AddHours(1);
                DateTime currentDate = DateTime.Now;
                var ts = new TimeSpan(currentDate.Ticks - timeStamp.Ticks);
                int daysAgo = ts.Days;
                return daysAgo;
            }
            catch
            {
                return 0;
            }
        }
        public static string GetRelativeTime(string timeStampString)
        {

            DateTime thisTime = DateTime.Now;
            bool isDaylight = TimeZoneInfo.Local.IsDaylightSavingTime(thisTime);
            string currentTimeZone = TimeZoneInfo.Local.StandardName;
            TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
            var timeStampDatetime = DateTime.ParseExact(timeStampString, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            DateTime timeStamp = TimeZoneInfo.ConvertTimeFromUtc(timeStampDatetime, localTimeZone);
            if (isDaylight)
                timeStamp = timeStamp.AddHours(1);



            DateTime currentDate = DateTime.Now;
            var ts = new TimeSpan(currentDate.Ticks - timeStamp.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return "Just Now";

            if (delta < 2 * MINUTE)
                return "a minute ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * MINUTE)
                return "an hour ago";

            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                return "yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " days ago";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }

        }

    }

}
