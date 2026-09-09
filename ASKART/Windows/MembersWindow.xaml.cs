using Askart.MockData;
using Askart.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Askart.Windows
{
    public partial class MembersWindow : Window
    {
        private Chat chat;

        public MembersWindow(Chat chat)
        {
            InitializeComponent();
            this.chat = chat;
            ChatNameText.Text = chat.Name;
            LoadMembers();
        }

        private void LoadMembers()
        {
            MembersPanel.Children.Clear();

            foreach (var member in chat.Members)
            {
                var user = DataInitializer.Users.FirstOrDefault(u => u.Id == member.UserId);
                if (user != null)
                {
                    var memberCard = CreateMemberCard(user, member.Role);
                    MembersPanel.Children.Add(memberCard);
                }
            }
        }

        private Border CreateMemberCard(User user, string role)
        {
            var border = new Border
            {
                Background = (Brush)FindResource("CardBackgroundBrush"),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(15, 15, 15, 15),
                Margin = new Thickness(0, 0, 0, 10),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 10,
                    ShadowDepth = 2,
                    Opacity = 0.1
                }
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Аватар
            var avatarBorder = new Border
            {
                Width = 50,
                Height = 50,
                CornerRadius = new CornerRadius(25),
                Background = new SolidColorBrush(Color.FromRgb(100, 150, 200)),
                VerticalAlignment = VerticalAlignment.Center
            };

            var avatarText = new TextBlock
            {
                Text = user.Name.Substring(0, 1).ToUpper(),
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            avatarBorder.Child = avatarText;
            Grid.SetColumn(avatarBorder, 0);

            // Информация
            var infoPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };

            var nameText = new TextBlock
            {
                Text = user.FullName,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("PrimaryTextBrush"),
                FontSize = 14
            };

            var emailText = new TextBlock
            {
                Text = user.Email,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 12,
                Foreground = (Brush)FindResource("SecondaryTextBrush")
            };

            infoPanel.Children.Add(nameText);
            infoPanel.Children.Add(emailText);

            Grid.SetColumn(infoPanel, 1);

            // Роль
            var roleBadge = new Border
            {
                Background = role == "Управляющий" ? (Brush)FindResource("ActiveBrush") : (Brush)FindResource("SuccessBrush"),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(12, 6, 12, 6),
                VerticalAlignment = VerticalAlignment.Center
            };

            var roleText = new TextBlock
            {
                Text = role,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };

            roleBadge.Child = roleText;
            Grid.SetColumn(roleBadge, 2);

            grid.Children.Add(avatarBorder);
            grid.Children.Add(infoPanel);
            grid.Children.Add(roleBadge);

            border.Child = grid;

            return border;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}