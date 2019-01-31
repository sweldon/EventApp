using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventApp.Services
{
    public interface CommentInterface<T>
    {
        Task<IEnumerable<T>> GetHolidayCommentsAsync(bool forceRefresh = false, string holidayId = null);
        Task<bool> AddComment(T comment);
        Task<T> GetCommentById(string id);
        string GetRelativeTime(DateTime commentDate);
    }

}
