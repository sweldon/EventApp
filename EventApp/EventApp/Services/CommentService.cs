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
        ObservableCollection<CommentList> allCommentThreads;
        Comment individualComment;
        CommentList commentGroup;
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

        public async Task<ObservableCollection<CommentList>> GetMoreComments(string holidayId = null, string user = null, string page="1")
        {

            allCommentThreads = new ObservableCollection<CommentList>();
            var values = new Dictionary<string, string>{
                   { "holiday", holidayId },
                   { "page", page },
            };
            if (isLoggedIn)
                values["username"] = currentUser;

            var content = new FormUrlEncodedContent(values);
            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/comments/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

            dynamic commentList = responseJSON.results;
            
            foreach (var thread in commentList)
            {
                commentGroup = new CommentList();
                foreach (var comment in thread)
                {
                    string TimeAgo = comment.time_since;
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

                    int padding = comment.depth;
                    Xamarin.Forms.Thickness paddingThickness = new
                        Xamarin.Forms.Thickness(Convert.ToDouble(
                            padding), 10, 10, 10
                            );

                    string voteStatus = comment.vote_status;
                    string UpVoteImage = Utils.GetUpVoteImage(voteStatus);
                    string DownVoteImage = Utils.GetDownVoteImage(voteStatus);
                    string author = comment.deleted == true ? "[deleted]" : comment.user;
                    string commentContent = comment.deleted == true ? "[deleted]" : comment.content;
                    string allowDelete = comment.deleted == true ? "false" : ShowDeleteVal;
                    string allowReply = comment.deleted == true ? "false" : ShowReplyVal;
                    double opacity = comment.deleted == true ? .2 : .5;
                    bool isEnabled = comment.deleted == true ? false : true;

                    commentGroup.Add(new Comment()
                    {
                        Id = comment.id,
                        Content = commentContent,
                        HolidayId = comment.holiday,
                        UserName = author,
                        TimeSince = TimeAgo,
                        ShowReply = allowReply,
                        ShowDelete = allowDelete,
                        Votes = comment.votes,
                        UpVoteStatus = UpVoteImage,
                        DownVoteStatus = DownVoteImage,
                        Parent = comment.parent,
                        ThreadPadding = paddingThickness,
                        ElementOpacity = opacity,
                        Enabled = isEnabled
                    });
                }
                allCommentThreads.Add(commentGroup);
            }
          return await Task.FromResult(allCommentThreads);
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
            
            dynamic commentList = responseJSON.results;
            
            foreach (var thread in commentList)
            {
                commentGroup = new CommentList();
                foreach (var comment in thread)
                {
                    string TimeAgo = comment.time_since;
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

                    int padding = comment.depth;
                    Xamarin.Forms.Thickness paddingThickness = new
                        Xamarin.Forms.Thickness(Convert.ToDouble(
                            padding), 10, 10, 10
                            );

                    string voteStatus = comment.vote_status;
                    string UpVoteImage = Utils.GetUpVoteImage(voteStatus);
                    string DownVoteImage = Utils.GetDownVoteImage(voteStatus);
                    string author = comment.deleted == true ? "[deleted]" : comment.user;
                    string commentContent = comment.deleted == true ? "[deleted]" : comment.content;
                    string allowDelete = comment.deleted == true ? "false" : ShowDeleteVal;
                    string allowReply = comment.deleted == true ? "false" : ShowReplyVal;
                    double opacity = comment.deleted == true ? .2 : .5;
                    bool isEnabled = comment.deleted == true ? false : true;
                    commentGroup.Add(new Comment()
                    {
                        Id = comment.id,
                        Content = commentContent,
                        HolidayId = comment.holiday,
                        UserName = author,
                        TimeSince = TimeAgo,
                        ShowReply = allowReply,
                        ShowDelete = allowDelete,
                        Votes = comment.votes,
                        UpVoteStatus = UpVoteImage, 
                        DownVoteStatus = DownVoteImage,
                        Parent = comment.parent,
                        ThreadPadding = paddingThickness,
                        ElementOpacity = opacity,
                        Enabled = isEnabled
                    });
                }

                comments.Insert(0, commentGroup);

            }

            return await Task.FromResult(comments);
        }

        public async Task VoteComment(string commentId, string vote)
        {
            var values = new Dictionary<string, string>{
                   { "comment", commentId },
                   { "username", currentUser },
                   { "vote", vote }
                };

            var content = new FormUrlEncodedContent(values);

            await App.globalClient.PostAsync(App.HolidailyHost +
                "/comments/" + commentId + "/", content);

        }

        public async Task<Comment> GetCommentById(string id)
        {
            var values = new Dictionary<string, string>{
                   { "id", id }
                };

            var content = new FormUrlEncodedContent(values);
            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/comments/"+id+"/", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
            dynamic commentJSON = responseJSON.results;
            
            try
            { 
                string TimeAgo = commentJSON.time_since;

                individualComment = new Comment() { Content = commentJSON.content, HolidayId = commentJSON.holiday_id, UserName = commentJSON.user, TimeSince = TimeAgo };
            }
            catch
            {
                individualComment = null;
            }


            return await Task.FromResult(individualComment);

        }



    }
}