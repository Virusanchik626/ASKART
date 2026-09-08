using System.Windows;
using System.Windows.Controls.Primitives;

namespace Askart.Windows
{
    public partial class ProfileWindow : Window
    {
        public ProfileWindow()
        {
            InitializeComponent();
            LoadUserData();
            ThemeToggle.IsChecked = App.IsDarkTheme;
        }

        private void LoadUserData()
        {
            if (App.CurrentUser != null)
            {
                NameText.Text = App.CurrentUser.FullName;
                EmailText.Text = App.CurrentUser.Email;
                PhoneText.Text = App.CurrentUser.Number;
            }
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            App.ToggleTheme();

            // Перезагружаем все окна для применения темы
            foreach (Window window in Application.Current.Windows)
            {
                if (window != this)
                {
                    window.Close();
                }
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                App.CurrentUser = null;

                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}