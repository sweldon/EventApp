using EventApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EventApp.Services
{
    public interface CommentInterface<T>
    {
        Task<IEnumerable<IEnumerable<T>>> GetHolidayCommentsAsync(bool forceRefresh = false, string holidayId = null, string user = null);
        Task<ObservableCollection<CommentList>> GetMoreComments(string holidayId = null, string user = null, string page="1");
        Task<T> GetCommentById(string id);
        Task VoteComment(string commentId, string vote);
    }

}
