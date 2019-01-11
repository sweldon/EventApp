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

    public class CommentService : ICommentStore<Comment>
    {

        List<Comment> comments;

        string ec2Instance = "http://ec2-54-156-187-51.compute-1.amazonaws.com";
        HttpClient client = new HttpClient();

        public CommentService()
        {


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
            var response = await client.PostAsync(ec2Instance + "/portal/get_comments/", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);

            dynamic commentList = responseJSON.CommentList;

            foreach (var comment in commentList)
            {
                comments.Insert(0, new Comment() { Id = comment.id, Content = comment.content, HolidayId = comment.holiday_id });
            }

            return await Task.FromResult(comments);
        }

    }
}