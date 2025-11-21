using NotificationModule.Enums;
using NotificationModule.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NotificationModule.Storage
{
    public class FileStorage : IStorage
    {
        private readonly string _filePath;

        public FileStorage(string filePath = "events.txt")
        {
            _filePath = filePath;
        }

        public List<LogEvent> LoadEvents()
        {
            var events = new List<LogEvent>();

            try
            {
                if (!File.Exists(_filePath))
                    return events;

                var lines = File.ReadAllLines(_filePath, Encoding.UTF8);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('|');
                    if (parts.Length >= 7)
                    {
                        var logEvent = new LogEvent
                        {
                            Timestamp = DateTime.Parse(parts[0]),
                            Level = (LogLevel)Enum.Parse(typeof(LogLevel), parts[1]),
                            EventType = (EventType)Enum.Parse(typeof(EventType), parts[2]),
                            User = parts[3],
                            ObjectId = parts[4],
                            Description = parts[5],
                            Module = parts[6]
                        };

                        if (parts.Length > 7)
                            logEvent.AdditionalData = parts[7];

                        events.Add(logEvent);
                    }
                }

                // Восстанавливаем ID по порядку
                for (int i = 0; i < events.Count; i++)
                {
                    events[i].Id = i + 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки событий: {ex.Message}");
            }

            return events;
        }

        public void SaveEvents(List<LogEvent> events)
        {
            try
            {
                // Создаем директорию если не существует
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var lines = events.Select(e =>
                    $"{e.Timestamp:yyyy-MM-dd HH:mm:ss.fff}|{e.Level}|{e.EventType}|{e.User}|{e.ObjectId}|{e.Description}|{e.Module}|{e.AdditionalData ?? ""}"
                );

                File.WriteAllLines(_filePath, lines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка сохранения событий: {ex.Message}", ex);
            }
        }

        public string GetStoragePath() => _filePath;

        public bool BackupExists() => File.Exists(_filePath + ".backup");

        public void CreateBackup()
        {
            if (File.Exists(_filePath))
            {
                File.Copy(_filePath, _filePath + ".backup", true);
            }
        }
    }
}
