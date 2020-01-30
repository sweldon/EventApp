using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventApp.Services
{
    public interface HolidayInterface<T>
    {
        Task<bool> AddItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<T> GetHolidayById(string id);
        Task<IEnumerable<T>> GetHolidaysAsync(bool forceRefresh = false);
        Task<IEnumerable<T>> GetTopHolidays(bool forceRefresh = false);
        Task VoteHoliday(string holidayId, string vote);
        Task<IEnumerable<T>> SearchHolidays(string holidayId);
    }

}
