using Askart.Models;
using System.Windows;

namespace Askart.Windows
{
    public partial class CreateTaskDialog : Window
    {

        public string TaskName { get; private set; }
        public string Description { get; private set; }
        public string Priority { get; private set; }
        public string SelectedColumn { get; private set; }
        public DateTime Deadline { get; private set; }

        public CreateTaskDialog(List<KanbanColumn> columns, string defaultColumn)
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
            columnComboBox.ItemsSource = columns.Select(column => column.Name).ToList();
            columnComboBox.SelectedIndex = 0;
            if (defaultColumn != null && columnComboBox.Items.Contains(defaultColumn))
                columnComboBox.SelectedItem = defaultColumn;
            priorityComboBox.ItemsSource = new[] { "Низкий", "Средний", "Высокий" };
            priorityComboBox.SelectedIndex = 1;
            deadlinePicker.SelectedDate = DateTime.Now.AddDays(7);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Введите название задачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (deadlinePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите срок выполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (columnComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите колонку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TaskName = nameTextBox.Text.Trim();
            Description = descTextBox.Text;
            Priority = "Средний";
            if (priorityComboBox.SelectedItem != null)
            {
                Priority = priorityComboBox.SelectedItem.ToString();
            }
            SelectedColumn = columnComboBox.SelectedItem.ToString();
            Deadline = deadlinePicker.SelectedDate.Value;
            DialogResult = true;
        }
    }
}
