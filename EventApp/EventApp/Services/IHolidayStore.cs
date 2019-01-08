using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventApp.Services
{
    public interface IHolidayStore<T>
    {
        Task<T> GetHolidayAsync(bool forceRefresh = false);
    }
}
