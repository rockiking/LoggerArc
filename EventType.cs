using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationModule.Enums
{
    public enum EventType
    {
        // Системные события
        ApplicationStart,
        ApplicationStop,

        // Бизнес-события💅
        CreateRecord,
        UpdateRecord,
        DeleteRecord,
        StatusChange,
        DeadlineChange,

        // Технические события
        DataLoad,
        DataSave,
        ConfigurationChange,

        // Ошибки
        AuthenticationError,
        ValidationError,
        SystemError
    }
}
