using Askart.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Askart.Windows
{
    public partial class PinnedMessagesWindow : Window
    {
        private Chat chat;

        public PinnedMessagesWindow(Chat chat)
        {
            InitializeComponent();
            this.chat = chat;
            ChatNameText.Text = chat.Name;
            LoadPinnedMessages();
        }

        private void LoadPinnedMessages()
        {
            PinnedPanel.Children.Clear();

            var pinnedMessages = chat.Messages.Where(m => m.IsPinned).OrderByDescending(m => m.SendTime).ToList();

            if (pinnedMessages.Count == 0)
            {
                var emptyText = new TextBlock
                {
                    Text = "В этом чате пока нет закреплённых сообщений",
                    FontFamily = (FontFamily)FindResource("Roboto"),
                    FontSize = 14,
                    Foreground = (Brush)FindResource("SecondaryTextBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 30, 0, 0)
                };
                PinnedPanel.Children.Add(emptyText);
                return;
            }

            foreach (var message in pinnedMessages)
            {
                PinnedPanel.Children.Add(CreatePinnedCard(message));
            }
        }

        private Border CreatePinnedCard(Message message)
        {
            var border = new Border
            {
                Background = (Brush)FindResource("CardBackgroundBrush"),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(15, 15, 15, 15),
                Margin = new Thickness(0, 0, 0, 10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7)),
                BorderThickness = new Thickness(3, 3, 3, 3),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 10,
                    ShadowDepth = 2,
                    Opacity = 0.15
                }
            };

            var stackPanel = new StackPanel();

            // Заголовок: автор + дата
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var authorText = new TextBlock
            {
                Text = "📌 " + message.SenderName,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                Foreground = (Brush)FindResource("PrimaryTextBrush"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(authorText, 0);

            var dateText = new TextBlock
            {
                Text = message.SendTime.ToString("dd.MM.yyyy HH:mm"),
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 11,
                Foreground = (Brush)FindResource("InactiveTextBrush"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(dateText, 1);

            headerGrid.Children.Add(authorText);
            headerGrid.Children.Add(dateText);
            stackPanel.Children.Add(headerGrid);

            // Разделитель
            var separator = new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                Margin = new Thickness(0, 10, 0, 10)
            };
            stackPanel.Children.Add(separator);

            // Текст сообщения
            var messageText = new TextBlock
            {
                Text = message.Text,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 14,
                Foreground = (Brush)FindResource("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            };
            stackPanel.Children.Add(messageText);

            // Индикатор редактирования
            if (message.EditTime.HasValue)
            {
                var editIndicator = new TextBlock
                {
                    Text = $"✏️ Отредактировано: {message.EditTime.Value:dd.MM.yyyy HH:mm}",
                    FontFamily = (FontFamily)FindResource("Roboto"),
                    FontSize = 11,
                    FontStyle = FontStyles.Italic,
                    Foreground = (Brush)FindResource("InactiveTextBrush"),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                stackPanel.Children.Add(editIndicator);
            }

            border.Child = stackPanel;

            return border;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}