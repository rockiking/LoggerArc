using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NotificationModule.Enums;
using Logger;

namespace NotificationModule.Models
{
    public class LogEvent// Представляет запись о событии в системе
    {
        public int Id { get; set; } //айдишник
        public DateTime Timestamp { get; set; }//время
        public LogLevel Level { get; set; }//уровень важности изменений
        public EventType EventType { get; set; }//Тип изменений
        public string User { get; set; }//идентфикатор пользователя, набудущее, пока не понимаю как его добавить:)
        public string ObjectId { get; set; }//объект(кстати есть классная кофейня с таким же названием) события
        public string Description { get; set; }//Описание
        public string Module { get; set; }//Модуль в котором произошло событие
        public string AdditionalData { get; set; }//Данные в формате JSON, хз зачем, друг сказал так надо и дип сик тоже, поэтому и тут

        public LogEvent()
        {
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {EventType} | " +
                   $"User: {User} | Object: {ObjectId} | {Description}";
        }
    }
}
