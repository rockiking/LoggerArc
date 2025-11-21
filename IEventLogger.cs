using NotificationModule.Enums;
using NotificationModule.Models;
using System;
using System.Collections.Generic;

namespace NotificationModule.Services
{
    public interface IEventLogger
    {
        void Log(LogLevel level, EventType eventType, string user, string objectId, string description, string module = "System");
        void LogInfo(EventType eventType, string user, string objectId, string description, string module = "System");
        void LogWarning(EventType eventType, string user, string objectId, string description, string module = "System");
        void LogError(EventType eventType, string user, string objectId, string description, string module = "System");

        List<LogEvent> GetEvents(DateTime from, DateTime to);
        List<LogEvent> GetEventsByLevel(LogLevel level);
        List<LogEvent> GetEventsByType(EventType eventType);
        List<LogEvent> GetEventsByUser(string user);
        List<LogEvent> GetEventsByModule(string module);
        List<LogEvent> GetAllEvents();

        int GetEventsCount();
        int GetEventsCount(DateTime from, DateTime to);
        Dictionary<LogLevel, int> GetLevelStatistics();
        Dictionary<EventType, int> GetTypeStatistics();
        Dictionary<string, int> GetUserStatistics();
        Dictionary<string, int> GetModuleStatistics();

        void ClearEvents();
        void ClearEventsOlderThan(DateTime date);
        bool ExportEvents(string filePath);

        List<LogEvent> SearchEvents(string searchText);
        List<LogEvent> GetRecentEvents(int count);
        bool HasEventsForObject(string objectId);
        List<LogEvent> GetObjectHistory(string objectId);

        event Action<LogEvent> OnNewEvent;
    }
}

