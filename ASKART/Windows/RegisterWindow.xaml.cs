using System.Windows;
using System.Windows.Input;
using Askart.MockData;
using Askart.Models;

namespace Askart.Windows
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string phone = PhoneTextBox?.Text.Trim() ?? "";

            // Валидация полей
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация почты
            if (!User.IsValidEmail(email))
            {
                MessageBox.Show("Неверный формат почты. Пример: user@example.com", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация пароля (минимум 8 символов)
            if (!User.IsValidPassword(password))
            {
                MessageBox.Show("Пароль должен содержать минимум 8 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация телефона (если указан)
            if (!string.IsNullOrWhiteSpace(phone) && !User.IsValidPhone(phone))
            {
                MessageBox.Show("Неверный формат телефона. Пример: +79991234567", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = DataInitializer.RegisterUser(firstName, lastName, email, password, phone);

            if (success)
            {
                MessageBox.Show("Регистрация успешна! Теперь вы можете войти.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Пользователь с такой почтой уже существует", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Login_Click(object sender, MouseButtonEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}