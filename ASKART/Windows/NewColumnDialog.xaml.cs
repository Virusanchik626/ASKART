using Askart.Models;
using System.Windows;

namespace Askart.Windows
{
    public partial class NewColumnDialog : Window
    {

        public string ColumnName { get; private set; }

        public NewColumnDialog()
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show("Введите название колонки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ColumnName = textBox.Text.Trim();
            DialogResult = true;
        }
    }
}
