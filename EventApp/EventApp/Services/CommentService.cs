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


        public async Task<IEnumerable<Comment>> GetHolidayCommentsAsync(bool forceRefresh = false, string holidayId = null, string user = null)
        {
            comments = new List<Comment>();

            var values = new Dictionary<string, string>{
                   { "holiday_id", holidayId },
                    { "user", currentUser }
                };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(App.HolidailyHost + "/portal/get_comments/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

            dynamic commentList = responseJSON.CommentList;

            foreach (var comment in commentList)
            {

                string commentTimestamp = comment.timestamp;

                string TimeAgo = Time.GetRelativeTime(commentTimestamp);
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
                // Maybe pass user id to this, and if it isnt none (theyre logged in) use it to set UpVote and DownVote
                comments.Insert(0, new Comment() { Id = comment.id, Content = comment.content, HolidayId = comment.holiday_id, UserName = comment.user, TimeSince = TimeAgo, ShowReply = ShowReplyVal, ShowDelete = ShowDeleteVal, Votes = comment.votes, UpVoteStatus=comment.up_vote_status, DownVoteStatus=comment.down_vote_status });
            }

            return await Task.FromResult(comments);
        }

        public async Task VoteComment(string commentId, string userName, string vote)
        {

            var values = new Dictionary<string, string>{
                   { "comment", commentId },
                   { "user", userName },
                   { "vote", vote }
                };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(App.HolidailyHost + "/portal/vote_comment/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

        }

        public async Task<Comment> GetCommentById(string id)
        {
            Debug.WriteLine(id);
            var values = new Dictionary<string, string>{
                   { "id", id }
                };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(App.HolidailyHost + "/portal/get_comment_by_id/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic commentJSON = responseJSON.comment;
            string commentTimestamp = commentJSON.timestamp;
            int statusCode = responseJSON.status_code;
            
            if(statusCode == 200)
            {

                string TimeAgo = Time.GetRelativeTime(commentTimestamp);

                individualComment = new Comment() { Content = commentJSON.content, HolidayId = commentJSON.holiday_id, UserName = commentJSON.user, TimeSince = TimeAgo };
            }
            else
            {
                individualComment = null;
            }


            return await Task.FromResult(individualComment);

        }



    }
}