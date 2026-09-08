using System.Windows;
using System.Windows.Input;

namespace Askart.Windows
{
    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }

        private void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Пожалуйста, введите вашу почту", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"Письмо для восстановления пароля отправлено на {email}",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BackToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}