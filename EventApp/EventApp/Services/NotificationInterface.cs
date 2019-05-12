using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventApp.Services
{
    public interface NotificationInterface<T>
    {
        Task<IEnumerable<T>> GetUserNotifications(bool forceRefresh = false);
    }

}
