using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApp.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Diagnostics;

namespace EventApp.Services
{
    public class NotificationService : NotificationInterface<Notification>
    {

        List<Holiday> items;
        List<Notification> notifications;
        Holiday individualHoliday;
        Dictionary<string, string> holidayResult;

        HttpClient client = new HttpClient();

        public string currentUser
        {
            get { return Settings.CurrentUser; }
        }

        public NotificationService()
        {


        }

        public async Task<IEnumerable<Notification>> GetUserNotifications(bool forceRefresh = false)
        {
            notifications = new List<Notification>();
            var values = new Dictionary<string, string>{
                   { "user", currentUser },
                };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(App.HolidailyHost + "/portal/get_user_notifications/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic notifList = responseJSON.Notifications;
            foreach (var n in notifList)
            {
                string type = n.type;
                string id = n.id;
                bool read = n.read;
                string commentContent = n.content;
                string notifTimestamp = n.timestamp;
                string currentTimeZone = TimeZone.CurrentTimeZone.StandardName;
                var commentDate = DateTime.ParseExact(notifTimestamp, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                DateTime localCommentDate = TimeZoneInfo.ConvertTimeFromUtc(commentDate, easternZone);
                string TimeAgo = GetRelativeTime(localCommentDate);

                notifications.Insert(0, new Notification() { Id = id, Type = type, Read = read, Content = commentContent, TimeSince = TimeAgo });
            }

            return await Task.FromResult(notifications);
        }

        public string GetRelativeTime(DateTime commentDate)
        {

            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            DateTime currentDate = DateTime.Now;
            var ts = new TimeSpan(currentDate.Ticks - commentDate.Ticks);
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
