using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventApp.Services
{
    public interface ICommentStore<T>
    {
        Task<IEnumerable<T>> GetHolidayCommentsAsync(bool forceRefresh = false, string holidayId = null);
        Task<bool> AddComment(T comment);
    }

}
