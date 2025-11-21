using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NotificationModule.Models;

namespace NotificationModule.Storage
{
    public interface IStorage
    {
        List<LogEvent> LoadEvents();
        void SaveEvents(List<LogEvent> events);
        string GetStoragePath();
    }
}
