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
                string TimeAgo = Time.GetRelativeTime(notifTimestamp);

                notifications.Insert(0, new Notification() { Id = id, Type = type, Read = read, Content = commentContent, TimeSince = TimeAgo });
            }

            return await Task.FromResult(notifications);
        }

        public async Task<IEnumerable<Notification>> GetUpdates(bool forceRefresh = false)
        {
            notifications = new List<Notification>();
            items = new List<Holiday>();
            var response = await client.GetAsync(App.HolidailyHost + "/portal/get_updates/");
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic notifList = responseJSON.Notifications;

            foreach (var n in notifList)
            {
                string updateContent = n.content;
                string notifTimestamp = n.timestamp;
                string TimeAgo = Time.GetRelativeTime(notifTimestamp);
                notifications.Insert(0, new Notification() {
                    Title = n.title,
                    Content = updateContent,
                    TimeSince = TimeAgo,
                    Author = n.author
                });
            }

            return await Task.FromResult(notifications);
        }

    }
}
