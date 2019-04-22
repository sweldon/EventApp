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

    public class CommentService : CommentInterface<Comment>
    {

        List<Comment> comments;
        Comment individualComment; 

        HttpClient client = new HttpClient();
        public string ShowReplyVal;
        public string ShowDeleteVal;
        public CommentService()
        {


        }


        public string currentUser
        {
            get { return Settings.CurrentUser; }
        }


        public async Task<bool> AddComment(Comment comment)
        {

            comments.Insert(0, comment);

            return await Task.FromResult(true);
        }


        public async Task<IEnumerable<Comment>> GetHolidayCommentsAsync(bool forceRefresh = false, string holidayId = null)
        {
            comments = new List<Comment>();

            var values = new Dictionary<string, string>{
                   { "holiday_id", holidayId }
                };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(App.HolidailyHost + "/portal/get_comments/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

            dynamic commentList = responseJSON.CommentList;

            foreach (var comment in commentList)
            {

                string commentTimestamp = comment.timestamp;
                string currentTimeZone = TimeZone.CurrentTimeZone.StandardName;
                var commentDate = DateTime.ParseExact(commentTimestamp, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                DateTime localCommentDate = TimeZoneInfo.ConvertTimeFromUtc(commentDate, easternZone);

                string TimeAgo = GetRelativeTime(localCommentDate);
                string commentUser = comment.user;
                if (String.Equals(commentUser, currentUser, StringComparison.OrdinalIgnoreCase)){
                    ShowReplyVal = "false";
                    ShowDeleteVal = "true";
                }
                else 
                {
                    ShowReplyVal = "true";
                    ShowDeleteVal = "false";
                }

                comments.Insert(0, new Comment() { Id = comment.id, Content = comment.content, HolidayId = comment.holiday_id, UserName = comment.user, TimeSince = TimeAgo, ShowReply = ShowReplyVal, ShowDelete = ShowDeleteVal });
            }

            return await Task.FromResult(comments);
        }

        public async Task<Comment> GetCommentById(string id)
        {

            var values = new Dictionary<string, string>{
                   { "id", id }
                };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(App.HolidailyHost + "/portal/get_comment_by_id/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

            string commentTimestamp = responseJSON.timestamp;
            string currentTimeZone = TimeZone.CurrentTimeZone.StandardName;
            Debug.WriteLine(currentTimeZone);
            var commentDate = DateTime.ParseExact(commentTimestamp, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
            DateTime localCommentDate = TimeZoneInfo.ConvertTimeFromUtc(commentDate, easternZone);

            string TimeAgo = GetRelativeTime(localCommentDate);

            individualComment = new Comment() { Content = responseJSON.content, HolidayId = responseJSON.holiday_id, UserName=responseJSON.user, TimeSince = TimeAgo };

            return await Task.FromResult(individualComment);

        }

        public string GetRelativeTime(DateTime commentDate) {

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