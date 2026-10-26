using Askart.Models;
using Askart.Services;
using System.Windows;
using System.Windows.Input;

namespace Askart.Windows
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string phone = PhoneTextBox.Text.Trim();

            
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            if (User.IsValidEmail(email) == false)
            {
                MessageBox.Show("Неверный формат почты. Пример: user@example.com", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            if (User.IsValidPassword(password) == false)
            {
                MessageBox.Show("Пароль должен содержать минимум 8 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            if (phone != "" && User.IsValidPhone(phone) == false)
            {
                MessageBox.Show("Неверный формат телефона. Пример: +79991234567", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            string error = DataService.Register(firstName, lastName, email, password, phone);

            if (error == null)
            {
                MessageBox.Show("Регистрация успешна! Теперь вы можете войти.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Login_Click(object sender, MouseButtonEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
