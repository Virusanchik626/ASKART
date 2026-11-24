using Askart.Models;
using Askart.Services;
using System.Windows;
using System.Windows.Controls;
namespace Askart.Windows
{
    public partial class ColumnSettingsWindow : Window
    {
        private Chat chat;
        private KanbanColumn column;

        
        private string[] colorNames = { "Серый", "Голубой", "Зелёный", "Красный", "Оранжевый", "Жёлтый",
                                        "Фиолетовый", "Розовый", "Синий", "Бирюзовый", "Тёмно-синий", "Коричневый" };
        private string[] colorCodes = { "#9E9E9E", "#5DD4FF", "#4CAF50", "#F44336", "#FF9800", "#FFEB3B",
                                        "#9C27B0", "#E91E63", "#2196F3", "#00BCD4", "#3F51B5", "#795548" };

        public ColumnSettingsWindow(Chat chat, KanbanColumn column)
        {
            ThemeHelper.Watch(this);

            this.chat = chat;
            this.column = column;

            InitializeComponent();
            nameTextBox.Text = column.Name;
            colorComboBox.ItemsSource = colorNames.Select((name, index) =>
                new ColumnColor(name, colorCodes[index])).ToList();
            colorComboBox.SelectedIndex = Math.Max(0, Array.FindIndex(colorCodes,
                code => string.Equals(code, column.Color, StringComparison.OrdinalIgnoreCase)));
        }

        public sealed record ColumnColor(string Name, string Code);

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColumnColor selectedItem = colorComboBox.SelectedItem as ColumnColor;
            if (selectedItem != null)
            {
                string hex = selectedItem.Code;
                previewBorder.Background = UiHelper.HexToBrush(hex);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                string newName = nameTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(newName))
                {
                    MessageBox.Show("Введите название колонки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
    
                string newColor = column.Color;
                ColumnColor selectedItem = colorComboBox.SelectedItem as ColumnColor;
                if (selectedItem != null)
                {
                    newColor = selectedItem.Code;
                }
    
                
                string error = await App.Client.Kanban.UpdateColumn(chat, column, newName, newColor);
                if (error != null)
                {
                    MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
    
                DialogResult = true;
            });
        }
    }

}
