using System.Windows;

namespace Askart.Windows
{
    public partial class ProfileWindow : Window
    {
        
        
        public bool IsLoggedOut { get; private set; }

        public ProfileWindow()
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
            IsLoggedOut = false;
            LoadUserData();
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

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                App.CurrentUser = null;
                IsLoggedOut = true;

                
                
                this.Close();
            }
        }
    }
}
