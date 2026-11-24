using Askart.Models;
using Askart.Services;
using System.Windows;
using System.Windows.Input;

namespace Askart.Windows
{
    public partial class LoginWindow : Window
    {
        private bool isSigningIn;
        private readonly CancellationTokenSource lifetime = new();

        public LoginWindow()
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
            Closed += (_, _) => lifetime.Cancel();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите почту и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (isSigningIn) return;
            isSigningIn = true;
            IsEnabled = false;
            try
            {
                var user = await App.Authentication.LoginAsync(email, password, lifetime.Token);
                if (IsLoaded) App.Navigation.OpenSession(this, user);
            }
            catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
            catch (AuthenticationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isSigningIn = false;
                IsEnabled = true;
            }
        }

        private void Register_Click(object sender, MouseButtonEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            App.Navigation.Replace(this, registerWindow);
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {


            ForgotPasswordWindow forgotWindow = new ForgotPasswordWindow();
            forgotWindow.Owner = this;
            forgotWindow.ShowDialog();
        }
    }
}