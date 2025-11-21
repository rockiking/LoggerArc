using NotificationModule.Services;
using NotificationModule.Storage;
using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Logger.Forms;

namespace NotificationModule.Forms
{
    public partial class MainForm : Form
    {
        private readonly EventLogger _logger;
        private int _requestCounter = 1;

        public MainForm()
        {
            InitializeComponent();

            // Инициализация логгера
            var storage = new FileStorage("events.txt");
            _logger = new EventLogger(storage);
            _logger.OnNewEvent += OnNewLogEvent;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateStatus();
            _logger.LogInfo(Enums.EventType.ApplicationStart, "System", "App", "Приложение запущено");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _logger.LogInfo(Enums.EventType.ApplicationStop, "System", "App", "Приложение завершено");
        }

        private void btnCreateRequest_Click(object sender, EventArgs e)
        {
            var requestId = $"REQ-{_requestCounter++:D4}";
            _logger.LogInfo(Enums.EventType.CreateRecord, "User1", requestId,
                "Создана новая заявка на оборудование", "RequestModule");
            MessageBox.Show($"Заявка {requestId} создана успешно!", "Создание заявки",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnChangeStatus_Click(object sender, EventArgs e)
        {
            var requestId = $"REQ-{new Random().Next(1, _requestCounter):D4}";
            _logger.LogInfo(Enums.EventType.StatusChange, "User2", requestId,
                "Статус изменен на 'В работе'", "RequestModule");
            MessageBox.Show($"Статус заявки {requestId} изменен!", "Изменение статуса",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            Фильтр filterForm = new Фильтр(_logger);
            filterForm.ShowDialog();
            try
            {
                var storage = new FileStorage("events.txt");
                string filePath = storage.GetStoragePath();

                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("notepad.exe", filePath); // Открываем файл в стандартном редакторе (Блокнот)
                }
                else
                {
                    MessageBox.Show("Файл логов не найден. Создайте сначала несколько событий.",
                                  "Файл не найден", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла логов: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSimulateError_Click(object sender, EventArgs e)
        {
            _logger.LogError(Enums.EventType.SystemError, "User3", "SYS-001",
                "Ошибка подключения к базе данных", "SystemModule");
            MessageBox.Show("Ошибка сымитирована и записана в лог!", "Симуляция ошибки",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void OnNewLogEvent(Models.LogEvent logEvent)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Models.LogEvent>(OnNewLogEvent), logEvent);
                return;
            }

            UpdateStatus();// Обновляем статусную строку при новом событии
        }

        private void UpdateStatus()
        {
            var count = _logger.GetAllEvents().Count; // Получаем общее количество событий из логгера
            lblStatus.Text = $"Всего событий: {count}";// Обновляем текст в статусной строке
        }
    }
}
