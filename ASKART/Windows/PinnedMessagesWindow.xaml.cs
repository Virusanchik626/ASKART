using Askart.Models;
using Askart.Services;
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
            ThemeHelper.Watch(this);
            InitializeComponent();
            this.chat = chat;
            ChatNameText.Text = chat.Name;
            LoadPinnedMessages();
        }

        private void LoadPinnedMessages()
        {
            PinnedPanel.Children.Clear();

            List<Message> pinnedMessages = DataService.GetPinnedMessages(chat);

            if (pinnedMessages.Count == 0)
            {
                TextBlock emptyText = UiHelper.MakeText("В этом чате пока нет закреплённых сообщений", 14, "SecondaryTextBrush");
                emptyText.HorizontalAlignment = HorizontalAlignment.Center;
                emptyText.Margin = new Thickness(0, 30, 0, 0);
                PinnedPanel.Children.Add(emptyText);
                return;
            }

            foreach (Message message in pinnedMessages)
            {
                PinnedPanel.Children.Add(CreatePinnedCard(message));
            }
        }

        private Border CreatePinnedCard(Message message)
        {
            Border border = new Border();
            border.SetResourceReference(Border.BackgroundProperty, "CardBackgroundBrush");
            border.CornerRadius = new CornerRadius(12);
            border.Padding = new Thickness(15);
            border.Margin = new Thickness(0, 0, 0, 10);
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7));
            border.BorderThickness = new Thickness(3);

            DropShadowEffect shadow = new DropShadowEffect();
            shadow.Color = Colors.Black;
            shadow.BlurRadius = 10;
            shadow.ShadowDepth = 2;
            shadow.Opacity = 0.15;
            border.Effect = shadow;

            StackPanel stackPanel = new StackPanel();

            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock authorText = UiHelper.MakeText("📌 " + message.SenderName, 13, "PrimaryTextBrush");
            authorText.FontWeight = FontWeights.SemiBold;
            authorText.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(authorText, 0);

            TextBlock dateText = UiHelper.MakeText(message.SendTime.ToString("dd.MM.yyyy HH:mm"), 11, "InactiveTextBrush");
            dateText.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(dateText, 1);

            headerGrid.Children.Add(authorText);
            headerGrid.Children.Add(dateText);
            stackPanel.Children.Add(headerGrid);

            Border separator = new Border();
            separator.Height = 1;
            separator.SetResourceReference(Border.BackgroundProperty, "LineBrush");
            separator.Margin = new Thickness(0, 10, 0, 10);
            stackPanel.Children.Add(separator);

            TextBlock messageText = UiHelper.MakeText(message.Text, 14, "PrimaryTextBrush");
            messageText.TextWrapping = TextWrapping.Wrap;
            stackPanel.Children.Add(messageText);

            if (message.EditTime.HasValue)
            {
                string editedText = "Отредактировано: " + message.EditTime.Value.ToString("dd.MM.yyyy HH:mm");
                TextBlock editIndicator = UiHelper.MakeText(editedText, 11, "InactiveTextBrush");
                editIndicator.FontStyle = FontStyles.Italic;
                editIndicator.HorizontalAlignment = HorizontalAlignment.Right;
                editIndicator.Margin = new Thickness(0, 5, 0, 0);
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
