using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EventApp.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;
using Xamarin.Forms;
using System.Linq;

namespace EventApp.Services
{
    public class GlobalServices
    {

        public static string currentUser
        {
            get { return Settings.CurrentUser; }
        }

        public static bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
        }

        // todo: load more not working
        public static async Task<ObservableCollection<Post>> GetPosts(string holidayId = null, bool buzz = false, int page = 0)
        {
            ObservableCollection<Post> HolidayPosts = new ObservableCollection<Post>();
            string url;

            if (buzz)
            {
                url = $"{App.HolidailyHost}/posts/?buzz=1&page={page}";
            }
            else
            {
                url = $"{App.HolidailyHost}/posts/?holiday_id={holidayId}";
            }

            if (isLoggedIn)
            {
                url = $"{url}&username={currentUser}";
            }

            var response = await App.globalClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic posts = responseJSON.results;
            string ShowDeleteVal;
            foreach (var p in posts)
            {
                string suffix = p.time_since_edit.ToString().Contains("now") ? "" : " ago";
                string TimeAgo = p.edited == null ? p.time_since : $"{p.time_since} (edited {p.time_since_edit}{suffix})";
                string postAuthor = p.user;
                if (String.Equals(postAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                {
                    ShowDeleteVal = "true";
                }
                else
                {
                    ShowDeleteVal = "false";
                }
                bool showReport = true;

                // If already blocked, reported or it's their own, don't allow another report
                if (p.blocked == true || p.reported == true ||
                    (String.Equals(postAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                    || p.deleted == true)
                {
                    showReport = false;
                }

                // Show user their own avatar, approved or not
                string avatar;
                if (p.user == App.GlobalUser.UserName && Settings.IsLoggedIn)
                {
                    avatar = App.GlobalUser.Avatar == null ? "default_user_128.png" : App.GlobalUser.Avatar;
                }
                else
                {
                    avatar = p.avatar == null ? "default_user_128.png" : p.avatar;
                }
                string image = p.image;
                bool isMediaVisible = string.IsNullOrEmpty(image) ? false : true;

                // post comments
                var comments = p.comments;
                ObservableCollection<Comment> PostComments = new ObservableCollection<Comment>();
                foreach (var comment in comments)
                {

                    string suffixComment = comment.time_since_edit.ToString().Contains("now") ? "" : " ago";
                    string TimeAgoComment = comment.edited == null ? comment.time_since :
                        $"{comment.time_since} (edited {comment.time_since_edit}{suffixComment})";
                    string commentUser = comment.user;
                    string avatarComment;
                    if (comment.UserName == App.GlobalUser.UserName && Settings.IsLoggedIn)
                    {
                        avatarComment = App.GlobalUser.Avatar;
                    }
                    else
                    {
                        avatarComment = comment.avatar == null ? "default_user_128.png" : comment.avatar;
                    }

                    string commentAuthor = comment.user;
                    string ShowDeleteComment;
                    if (String.Equals(commentAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                    {

                        ShowDeleteComment = "true";
                    }
                    else
                    {

                        ShowDeleteComment = "false";
                    }
                    bool showReportComment = true;

                    // If already blocked, reported or it's their own, don't allow another report
                    if (comment.blocked == true || comment.reported == true ||
                        (String.Equals(commentAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                        || comment.deleted == true)
                    {
                        showReportComment = false;
                    }

                    dynamic replies = comment.replies;


                    PostComments.Add(new Comment()
                    {
                        Id = comment.id,
                        Content = comment.content,
                        HolidayId = comment.holiday,
                        UserName = comment.user,
                        TimeSince = TimeAgoComment,
                        Avatar = avatarComment,
                        ShowEdit = ShowDeleteComment, // If you can delete, you can edit
                        ShowDelete = ShowDeleteComment,
                        ShowReport = showReportComment,

                        LikeImage = comment.liked == true ? "like_active.png" : "like_neutral.png",
                        Likes = comment.likes,
                        LikeTextColor = comment.liked == true ? Color.FromHex("4c96e8") : Color.FromHex("808080"),
                        LikeEnabled = true,
                        ShowReactions = comment.likes > 0 ? true : false,

                    });
                    // Append replies
                    foreach (var r in replies)
                    {

                        string avatarReply;
                        string replyAuthor = r.user;
                        if (r.UserName == App.GlobalUser.UserName && Settings.IsLoggedIn)
                        {
                            avatarReply = App.GlobalUser.Avatar;
                        }
                        else
                        {
                            avatarReply = r.avatar == null ? "default_user_128.png" : r.avatar;
                        }
                        string ShowDeleteReply;
                        if (String.Equals(replyAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                        {

                            ShowDeleteReply = "true";
                        }
                        else
                        {

                            ShowDeleteReply = "false";
                        }
                        bool showReportReply = true;

                        // If already blocked, reported or it's their own, don't allow another report
                        if (r.blocked == true || r.reported == true ||
                            (String.Equals(replyAuthor, currentUser, StringComparison.OrdinalIgnoreCase))
                            || r.deleted == true)
                        {
                            showReportReply = false;
                        }
                        Thickness paddingThickness = new Thickness(Convert.ToDouble(20), 0, 0, 0);

                        string suffixReply = r.time_since_edit.ToString().Contains("now") ? "" : " ago";
                        string TimeAgoReply = r.edited == null ? r.time_since :
                            $"{r.time_since} (edited {r.time_since_edit}{suffixReply})";

                        PostComments.Add(new Comment()
                        {
                            Id = r.id,
                            Content = r.content,
                            HolidayId = r.holiday,
                            UserName = replyAuthor,
                            TimeSince = TimeAgoReply,
                            Avatar = avatarReply,
                            ShowEdit = ShowDeleteReply, // If you can delete, you can edit
                            ShowDelete = ShowDeleteReply,
                            ShowReport = showReportReply,
                            ThreadPadding = paddingThickness,

                            LikeImage = r.liked == true ? "like_active.png" : "like_neutral.png",
                            Likes = r.likes,
                            LikeTextColor = r.liked == true ? Color.FromHex("4c96e8") : Color.FromHex("808080"),
                            LikeEnabled = true,
                            ShowReactions = r.likes > 0 ? true : false

                        });
                    }

                }

                HolidayPosts.Add(new Post()
                {
                    Id = p.id,
                    Content = p.content,
                    HolidayId = p.holiday,
                    UserName = p.user,
                    TimeSince = TimeAgo,
                    ShowEdit = ShowDeleteVal, // If you can delete, you can edit
                    ShowDelete = ShowDeleteVal,
                    ShowReport = showReport,

                    Avatar = avatar,
                    Image = p.image,
                    ShowImage = isMediaVisible,
                    LikeImage = p.liked == true ? "like_active.png" : "like_neutral.png",
                    Likes = p.likes,
                    LikeTextColor = p.liked == true ? Color.FromHex("4c96e8") : Color.FromHex("808080"),
                    LikeEnabled = true,
                    ShowReactions = p.likes > 0 ? true : false,
                    ShowComments = PostComments.Count() > 0 ? true : false,
                    Comments = PostComments,
                    HolidayName = p.holiday_name
                });


            }
            return await Task.FromResult(HolidayPosts);
        }

        public static async Task<ObservableCollection<Holiday>> GetHolidaysAsync(int page = 0, bool past = false)
        {
            ObservableCollection<Holiday> items = new ObservableCollection<Holiday>();
            var values = new Dictionary<string, string>{
                { "page", page.ToString() }
            };

            if (isLoggedIn)
                values["username"] = currentUser;

            if (past)
                values["past"] = "true";

            var content = new FormUrlEncodedContent(values);
            int TotalNumberOfAttempts = 12;
            int numberOfAttempts = 0;
            dynamic response = null;
            while (response == null)
            {
                try
                {
                    response = await App.globalClient.PostAsync(App.HolidailyHost + "/holidays/", content);
                }
                catch
                {
                    numberOfAttempts++;
                    if (numberOfAttempts >= TotalNumberOfAttempts)
                        throw;
                    await Task.Delay(5000);
                }
            }


            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic holidayList = responseJSON.results;
            bool todayDone = false;
            foreach (var holiday in holidayList)
            {
                string holidayDescription = holiday.description;
                string HolidayDescriptionShort = holidayDescription.Length <= 90 ? holidayDescription : holidayDescription.Substring(0, 90) + "...";

                if (holiday.time_since != "Today" && !todayDone && page == 0 && !past)
                {
                    todayDone = true;
                    items.Add(new Holiday()
                    {
                        Id = "-1",
                        ShowAd = true,
                        ShowHolidayContent = false,
                    });


                }

                items.Add(new Holiday()
                {
                    Id = holiday.id,
                    Name = holiday.name,
                    Description = holiday.description,
                    NumComments = holiday.num_comments,
                    TimeSince = holiday.time_since,
                    DescriptionShort = HolidayDescriptionShort,
                    HolidayImage = holiday.image,
                    ShowAd = false,
                    ShowHolidayContent = true,
                    Date = holiday.date,
                    Votes = holiday.votes,
                    CelebrateStatus = holiday.celebrating == true ? "celebrate_active.png" : "celebrate.png",
                    Blurb = holiday.blurb,
                    Active = holiday.active
                });

            }

            return await Task.FromResult(items);
        }

        public static async Task VoteHoliday(string holidayId, string vote)
        {

            var values = new Dictionary<string, string>{
                   { "username", currentUser },
                   { "vote", vote }
                };

            var content = new FormUrlEncodedContent(values);
            await App.globalClient.PostAsync(App.HolidailyHost + "/holidays/" + holidayId + "/", content);

        }
    }
}

