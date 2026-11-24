using System.Windows;

namespace Askart.Windows
{
    public partial class EditMessageDialog : Window
    {

        public string NewText { get; private set; }

        public EditMessageDialog(string oldText)
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
            textBox.Text = oldText;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show("Сообщение не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewText = textBox.Text;
            DialogResult = true;
        }
    }
}
