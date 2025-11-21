using NotificationModule.Services;
using System;
using System.Windows.Forms;
using NotificationModule.Enums;
using NotificationModule.Models;
using NotificationModule.Storage;
using System.Linq;
using System.Collections.Generic;
namespace Logger.Forms
{
    public partial class Фильтр : Form
    {
        private readonly EventLogger _logger;
        public Фильтр(EventLogger logger)
        {
            InitializeComponent();
            _logger = logger;// Сохраняем переданный логгер в поле класса
            InitializeFilters();
            LoadLogs();// Загружаем данные в таблицу при открытии формы
        }
        private void InitializeFilters() 
        {
            comboBox1.DataSource = Enum.GetValues(typeof(LogLevel));// Заполняем выпадающий список уровнями логирования (Info, Warning, Error)
            comboBox1.SelectedIndex = -1;
            
            comboBox2.DataSource = Enum.GetValues(typeof(EventType));// Заполняем выпадающий список типами событий
            comboBox2.SelectedIndex = -1;

            dateTimePicker1.Value = DateTime.Now.AddDays(-7);// Устанавливаем начальную дату фильтрации (7 дней назад)
            dateTimePicker2.Value = DateTime.Now;

            RefreshUserAndModuleLists();// Загружаем списки пользователей и модулей из базы данных
            


        }
        private void RefreshUserAndModuleLists()// Метод загрузки списков пользователей и модулей для фильтров
        {
            try
            { 
                var users = _logger.GetUserStatistics().Keys.ToList(); // Получаем список уникальных пользователей из логгера
                var modules = _logger.GetModuleStatistics().Keys.ToList(); // Получаем список уникальных модулей из логгера 
                comboBox3.DataSource = new[] { "Все пользователи" }.Concat(users).ToList();// Добавляем в начало списка пользователей вариант "Все пользователи"
                comboBox4.DataSource = new[] { "Все модули" }.Concat(modules).ToList(); // Добавляем в начало списка модулей вариант "Все модули"
                comboBox3.SelectedIndex = 0;// Выбираем первый элемент в списке пользователей ("Все пользователи")
                comboBox4.SelectedIndex = 0;// Выбираем первый элемент в списке модулей ("Все модули")
            }
            catch (Exception ex)
            {
               
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); // Если произошла ошибка при загрузке, показываем сообщение
            }
        }
        private void LoadLogs()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                DateTime fromDate = dateTimePicker1.Value.Date;// Получаем начальную дату из фильтра (начало дня)
                DateTime toDate = dateTimePicker2.Value.Date.AddDays(1).AddSeconds(-1);// Получаем конечную дату из фильтра (конец дня)
                var events = _logger.GetEvents(fromDate, toDate);// Получаем события за выбранный период из логгера
                if (comboBox1.SelectedIndex >= 0)// Фильтрация по уровню важности, если выбран конкретный уровень
                {
                    var selectedLevel = (LogLevel)comboBox1.SelectedItem;
                    events = events.Where(e => e.Level == selectedLevel).ToList();
                }
                if (comboBox2.SelectedIndex >= 0)// Фильтрация по типу события, если выбран конкретный тип
                {
                    var selectedType = (EventType)comboBox2.SelectedItem;
                    events = events.Where(e => e.EventType == selectedType).ToList();
                }
                if (comboBox3.SelectedIndex > 0)// Фильтрация по пользователю, если выбран конкретный пользователь
                {
                    var selectedUser = comboBox3.SelectedItem.ToString();
                    events = events.Where(e => e.User == selectedUser).ToList();
                }
                if (comboBox4.SelectedIndex > 0)// Фильтрация по модулю, если выбран конкретный модуль
                {
                    var selectedModule = comboBox4.SelectedItem.ToString();
                    events = events.Where(e => e.Module == selectedModule).ToList();
                }
                if (!string.IsNullOrWhiteSpace(textBox1.Text))// Фильтрация по текстовому поиску, если введен текст
                {
                    var searchResults = _logger.SearchEvents(textBox1.Text);
                    events = events.Where(e => searchResults.Any(r => r.Id == e.Id)).ToList();
                }
                DisplayEventsInGrid(events);// Отображаем отфильтрованные события в таблице
                UpdateStatistics(events);// Обновляем статистику (количество событий)
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки логов: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        private void UpdateStatistics(List<LogEvent> events)// Метод обновления статистики (количества событий)
        {
            if (lblTotal != null)
            {
                lblTotal.Text = $"Найдено событий: {events.Count}";
            }
            else
            {
               
                this.Text = $"Просмотр логов - {events.Count} событий";
            }
        }
        private void DisplayEventsInGrid(List<LogEvent> events) // Метод отображения событий в таблице
        {
            dataGridView1.Rows.Clear();// Очищаем все строки в таблице перед добавлением новых

            foreach (var logEvent in events)
            {
                dataGridView1.Rows.Add(
                    logEvent.Id,
                    logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    logEvent.Level,
                    logEvent.EventType,
                    logEvent.User,
                    logEvent.ObjectId,
                    logEvent.Description,
                    logEvent.Module
                );
            }
        }

        private void Фильтр_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadLogs();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            textBox1.Clear();
            dateTimePicker1.Value = DateTime.Now.AddDays(-7);
            dateTimePicker2.Value = DateTime.Now;

            LoadLogs();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Text files (*.txt)|*.txt";
            saveDialog.FileName = $"logs_export_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                bool success = _logger.ExportEvents(saveDialog.FileName);
                if (success)
                {
                    MessageBox.Show($"Логи экспортированы в: {saveDialog.FileName}", "Успех");
                }
                else
                {
                    MessageBox.Show("Ошибка экспорта", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)// Обработчик нажатия клавиш в поле текстового поиска
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                LoadLogs();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                var row = dataGridView1.Rows[e.RowIndex]; // Получаем строку, по которой кликнули
                string details = $"ID: {row.Cells[0].Value}\n" +
                               $"Время: {row.Cells[1].Value}\n" +
                               $"Уровень: {row.Cells[2].Value}\n" +
                               $"Тип: {row.Cells[3].Value}\n" +
                               $"Пользователь: {row.Cells[4].Value}\n" +
                               $"Объект: {row.Cells[5].Value}\n" +
                               $"Описание: {row.Cells[6].Value}\n" +
                               $"Модуль: {row.Cells[7].Value}";

                MessageBox.Show(details, "Детали события", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
    }
    

