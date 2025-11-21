using NotificationModule.Enums;
using NotificationModule.Models;
using NotificationModule.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NotificationModule.Services
{
    public class EventLogger : IEventLogger
    {
        private List<LogEvent> _events;
        private readonly IStorage _storage;
        private int _nextId;
        private readonly object _lockObject = new object();

        public event Action<LogEvent> OnNewEvent;

        public EventLogger(IStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            InitializeFromStorage();
        }

        private void InitializeFromStorage()
        {
            lock (_lockObject)
            {
                _events = _storage.LoadEvents();
                _nextId = _events.Count > 0 ? _events.Max(e => e.Id) + 1 : 1;
            }
        }

        //основные методы логгирования

        public void Log(LogLevel level, EventType eventType, string user,
                       string objectId, string description, string module = "System")
        {
            if (string.IsNullOrWhiteSpace(user))
                throw new ArgumentException("User cannot be null or empty", nameof(user));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be null or empty", nameof(description));

            var logEvent = new LogEvent
            {
                Id = GetNextId(),
                Level = level,
                EventType = eventType,
                User = user.Trim(),
                ObjectId = objectId?.Trim() ?? "N/A",
                Description = description.Trim(),
                Module = module?.Trim() ?? "System"
            };

            AddEvent(logEvent);
        }

        public void LogInfo(EventType eventType, string user, string objectId,
                           string description, string module = "System")
        {
            Log(LogLevel.Info, eventType, user, objectId, description, module);
        }

        public void LogWarning(EventType eventType, string user, string objectId,
                              string description, string module = "System")
        {
            Log(LogLevel.Warning, eventType, user, objectId, description, module);
        }

        public void LogError(EventType eventType, string user, string objectId,
                            string description, string module = "System")
        {
            Log(LogLevel.Error, eventType, user, objectId, description, module);
        }

        //Метод для получения событий

        public List<LogEvent> GetEvents(DateTime from, DateTime to)
        {
            lock (_lockObject)
            {
                return _events
                    .Where(e => e.Timestamp >= from && e.Timestamp <= to)
                    .OrderByDescending(e => e.Timestamp)
                    .ToList();
            }
        }

        public List<LogEvent> GetEventsByLevel(LogLevel level)
        {
            lock (_lockObject)
            {
                return _events
                    .Where(e => e.Level == level)
                    .OrderByDescending(e => e.Timestamp)
                    .ToList();
            }
        }

        public List<LogEvent> GetEventsByType(EventType eventType)
        {
            lock (_lockObject)
            {
                return _events
                    .Where(e => e.EventType == eventType)
                    .OrderByDescending(e => e.Timestamp)
                    .ToList();
            }
        }

        public List<LogEvent> GetEventsByUser(string user)
        {
            if (string.IsNullOrWhiteSpace(user))
                return new List<LogEvent>();

            lock (_lockObject)
            {
                return _events
                    .Where(e => e.User.Equals(user.Trim(), StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(e => e.Timestamp)
                    .ToList();
            }
        }

        public List<LogEvent> GetEventsByModule(string module)
        {
            if (string.IsNullOrWhiteSpace(module))
                return new List<LogEvent>();

            lock (_lockObject)
            {
                return _events
                    .Where(e => e.Module.Equals(module.Trim(), StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(e => e.Timestamp)
                    .ToList();
            }
        }

        public List<LogEvent> GetAllEvents()
        {
            lock (_lockObject)
            {
                return new List<LogEvent>(_events.OrderByDescending(e => e.Timestamp));
            }
        }

        //Статистика и аналитика

        public int GetEventsCount()
        {
            lock (_lockObject)
            {
                return _events.Count;
            }
        }

        public int GetEventsCount(DateTime from, DateTime to)
        {
            lock (_lockObject)
            {
                return _events.Count(e => e.Timestamp >= from && e.Timestamp <= to);
            }
        }

        public Dictionary<LogLevel, int> GetLevelStatistics()
        {
            lock (_lockObject)
            {
                return _events
                    .GroupBy(e => e.Level)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Count()
                    );
            }
        }

        public Dictionary<EventType, int> GetTypeStatistics()
        {
            lock (_lockObject)
            {
                return _events
                    .GroupBy(e => e.EventType)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Count()
                    );
            }
        }

        public Dictionary<string, int> GetUserStatistics()
        {
            lock (_lockObject)
            {
                return _events
                    .GroupBy(e => e.User)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Count()
                    );
            }
        }

        public Dictionary<string, int> GetModuleStatistics()
        {
            lock (_lockObject)
            {
                return _events
                    .GroupBy(e => e.Module)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Count()
                    );
            }
        }

        //Манимуляции с приложением

        public void ClearEvents()
        {
            lock (_lockObject)
            {
                _events.Clear();
                _nextId = 1;
                _storage.SaveEvents(_events);
            }
        }

        public void ClearEventsOlderThan(DateTime date)
        {
            lock (_lockObject)
            {
                int removedCount = _events.RemoveAll(e => e.Timestamp < date);
                if (removedCount > 0)
                {
                    // Пересчитываем ID чтобы не было пропусков
                    for (int i = 0; i < _events.Count; i++)
                    {
                        _events[i].Id = i + 1;
                    }
                    _nextId = _events.Count + 1;
                    _storage.SaveEvents(_events);
                }
            }
        }

        public bool ExportEvents(string filePath)
        {
            try
            {
                var exportStorage = new FileStorage(filePath);
                exportStorage.SaveEvents(GetAllEvents());
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка экспорта: {ex.Message}");
                return false;
            }
        }

        //Вспомогательные методы

        private void AddEvent(LogEvent logEvent)
        {
            lock (_lockObject)
            {
                _events.Add(logEvent);
                _storage.SaveEvents(_events);
            }

            // Уведомляем юзеров о новом событии
            OnNewEvent?.Invoke(logEvent);

            // Дублируем в консоль для отладки
            System.Diagnostics.Debug.WriteLine(logEvent.ToString());
        }

        private int GetNextId()
        {
            return _nextId++;
        }

        //Доп методы, для удобства
        public List<LogEvent> SearchEvents(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<LogEvent>();

            lock (_lockObject)
            {
                return _events
                    .Where(e => e.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               e.User.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               e.ObjectId.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderByDescending(e => e.Timestamp)
                    .ToList();
            }
        }
        public List<LogEvent> GetRecentEvents(int count)
        {
            lock (_lockObject)
            {
                return _events
                    .OrderByDescending(e => e.Timestamp)
                    .Take(count)
                    .ToList();
            }
        }
        public bool HasEventsForObject(string objectId)
        {
            if (string.IsNullOrWhiteSpace(objectId))
                return false;

            lock (_lockObject)
            {
                return _events.Any(e => e.ObjectId.Equals(objectId, StringComparison.OrdinalIgnoreCase));
            }
        }
        public List<LogEvent> GetObjectHistory(string objectId)
        {
            if (string.IsNullOrWhiteSpace(objectId))
                return new List<LogEvent>();

            lock (_lockObject)
            {
                return _events
                    .Where(e => e.ObjectId.Equals(objectId, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(e => e.Timestamp)
                    .ToList();
            }
        }
    }
}