using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventApp.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace EventApp.Services
{



    public class CommentService : CommentInterface<Comment>
    {


        ObservableCollection<ObservableCollection<Comment>> comments;
        Comment individualComment;
        ObservableCollection<Comment> commentGroup;

        public string ShowReplyVal;
        public string ShowDeleteVal;
        public CommentService()
        {

        }

        public string currentUser
        {
            get { return Settings.CurrentUser; }
        }

        public bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
        }

        public async Task<IEnumerable<IEnumerable<Comment>>> GetHolidayCommentsAsync(bool forceRefresh = false, string holidayId = null, string user = null)
        {
            comments = new ObservableCollection<ObservableCollection<Comment>>();

            var values = new Dictionary<string, string>{
                   { "holiday", holidayId },
            };
            if (isLoggedIn)
                values["username"] = currentUser;

            var content = new FormUrlEncodedContent(values);
            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/comments/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

            dynamic commentList = responseJSON.CommentList;

            foreach (var thread in commentList)
            {
                commentGroup = new ObservableCollection<Comment>();
                foreach (var comment in thread)
                {
                    string commentTimestamp = comment.timestamp;

                    string TimeAgo = Time.GetRelativeTime(commentTimestamp);
                    string commentUser = comment.user;
                    if (String.Equals(commentUser, currentUser, StringComparison.OrdinalIgnoreCase))
                    {
                        ShowReplyVal = "false";
                        ShowDeleteVal = "true";
                    }
                    else
                    {
                        ShowReplyVal = "true";
                        ShowDeleteVal = "false";
                    }

                    string padding = comment.padding;
                    string[] paddingVals = padding.Split(',');
                    Xamarin.Forms.Thickness paddingThickness = new Xamarin.Forms.Thickness(Convert.ToDouble(paddingVals[0]),
                                                                                            Convert.ToDouble(paddingVals[1]),
                                                                                            Convert.ToDouble(paddingVals[2]),
                                                                                            Convert.ToDouble(paddingVals[3]));
                    commentGroup.Add(new Comment()
                    {
                        Id = comment.id,
                        Content = comment.content,
                        HolidayId = comment.holiday_id,
                        UserName = comment.user,
                        TimeSince = TimeAgo,
                        ShowReply = ShowReplyVal,
                        ShowDelete = ShowDeleteVal,
                        Votes = comment.votes,
                        UpVoteStatus = comment.up_vote_status,
                        DownVoteStatus = comment.down_vote_status,
                        Parent = comment.parent,
                        ThreadPadding = paddingThickness
                    });
                }

                comments.Insert(0, commentGroup);

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

            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/portal/vote_comment/", content);
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
            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/portal/get_comment_by_id/", content);
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