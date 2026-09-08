using Askart.MockData;
using Askart;
using Askart.Windows;
using System.Windows;
using System.Windows.Input;

namespace Askart.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите почту и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = DataInitializer.Authenticate(email, password);

            if (user != null)
            {
                App.CurrentUser = user;

                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверная почта или пароль", "Ошибка входа",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Register_Click(object sender, MouseButtonEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            var forgotWindow = new ForgotPasswordWindow();
            forgotWindow.Show();
        }
    }
}