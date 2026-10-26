using Askart.Models;
using Askart.Services;
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
            ThemeHelper.Watch(this);
            InitializeComponent();
            this.chat = chat;
            ChatNameText.Text = chat.Name;
            LoadMembers();
        }

        private void LoadMembers()
        {
            MembersPanel.Children.Clear();

            List<ChatMember> members = DataService.GetMembers(chat);

            foreach (ChatMember member in members)
            {
                User user = DataService.GetUserById(member.UserId);
                if (user != null)
                {
                    Border memberCard = CreateMemberCard(user, member.Role);
                    MembersPanel.Children.Add(memberCard);
                }
            }
        }

        private Border CreateMemberCard(User user, string role)
        {
            Border border = new Border();
            border.SetResourceReference(Border.BackgroundProperty, "CardBackgroundBrush");
            border.CornerRadius = new CornerRadius(12);
            border.Padding = new Thickness(15);
            border.Margin = new Thickness(0, 0, 0, 10);

            DropShadowEffect shadow = new DropShadowEffect();
            shadow.Color = Colors.Black;
            shadow.BlurRadius = 10;
            shadow.ShadowDepth = 2;
            shadow.Opacity = 0.1;
            border.Effect = shadow;

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            
            Border avatarBorder = new Border();
            avatarBorder.Width = 50;
            avatarBorder.Height = 50;
            avatarBorder.CornerRadius = new CornerRadius(25);
            avatarBorder.Background = new SolidColorBrush(Color.FromRgb(100, 150, 200));
            avatarBorder.VerticalAlignment = VerticalAlignment.Center;

            TextBlock avatarText = new TextBlock();
            avatarText.Text = user.Name.Substring(0, 1).ToUpper();
            avatarText.FontSize = 22;
            avatarText.FontWeight = FontWeights.Bold;
            avatarText.Foreground = Brushes.White;
            avatarText.VerticalAlignment = VerticalAlignment.Center;
            avatarText.HorizontalAlignment = HorizontalAlignment.Center;
            avatarBorder.Child = avatarText;
            Grid.SetColumn(avatarBorder, 0);

            
            StackPanel infoPanel = new StackPanel();
            infoPanel.VerticalAlignment = VerticalAlignment.Center;
            infoPanel.Margin = new Thickness(15, 0, 0, 0);

            TextBlock nameText = UiHelper.MakeText(user.FullName, 14, "PrimaryTextBrush");
            nameText.FontWeight = FontWeights.SemiBold;

            TextBlock emailText = UiHelper.MakeText(user.Email, 12, "SecondaryTextBrush");

            infoPanel.Children.Add(nameText);
            infoPanel.Children.Add(emailText);
            Grid.SetColumn(infoPanel, 1);

            
            
            
            Border roleBadge = new Border();
            if (role == "Владелец" || role == "Администратор")
            {
                roleBadge.SetResourceReference(Border.BackgroundProperty, "ActiveBrush");
            }
            else
            {
                roleBadge.SetResourceReference(Border.BackgroundProperty, "SuccessBrush");
            }
            roleBadge.CornerRadius = new CornerRadius(15);
            roleBadge.Padding = new Thickness(12, 6, 12, 6);
            roleBadge.VerticalAlignment = VerticalAlignment.Center;

            TextBlock roleText = new TextBlock();
            roleText.Text = role;
            roleText.FontFamily = UiHelper.GetFont();
            roleText.FontSize = 12;
            roleText.FontWeight = FontWeights.SemiBold;
            roleText.Foreground = Brushes.White;
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
