using System.Collections.Generic;
using System.Threading.Tasks;
using EventApp.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace EventApp.Services
{
    public class NotificationService : NotificationInterface<Notification>
    {

        List<Holiday> items;
        List<Notification> notifications;

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
                   { "username", currentUser },
                };

            var content = new FormUrlEncodedContent(values);

            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/notifications/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic notifList = responseJSON.results;
            foreach (var n in notifList)
            {
                notifications.Insert(0, new Notification() {
                    Id = n.notification_id,
                    Type = n.notification_type,
                    Read = n.read,
                    Content = n.content,
                    TimeSince = n.time_since });
            }

            return await Task.FromResult(notifications);
        }

        public async Task<IEnumerable<Notification>> GetUpdates(bool forceRefresh = false)
        {
            notifications = new List<Notification>();
            items = new List<Holiday>();
            var response = await App.globalClient.GetAsync(App.HolidailyHost + "/news");
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic notifList = responseJSON.results;

            foreach (var n in notifList)
            {
                notifications.Insert(0, new Notification() {
                    Title = n.title,
                    Content = n.content,
                    TimeSince = n.time_since,
                    Author = n.author
                });
            }
            return await Task.FromResult(notifications);
        }

    }
}
