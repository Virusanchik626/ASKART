using System.Windows;

namespace Askart.Windows
{
    public partial class CreateChatDialog : Window
    {

        public string ChatName { get; private set; }
        public bool IsGroup { get; private set; }
        public bool IsWorkChat { get; private set; }

        public CreateChatDialog()
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Введите название чата", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ChatName = nameTextBox.Text.Trim();
            IsGroup = (groupRadio.IsChecked == true || workRadio.IsChecked == true);
            IsWorkChat = (workRadio.IsChecked == true);
            DialogResult = true;
        }
    }
}
