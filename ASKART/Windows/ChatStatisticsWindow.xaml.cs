using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Askart.Models;

namespace Askart.Windows
{
    public partial class ChatStatisticsWindow : Window
    {
        private Chat chat;

        public ChatStatisticsWindow(Chat chat)
        {
            InitializeComponent();
            this.chat = chat;
            ChatNameText.Text = chat.Name;
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            StatsPanel.Children.Clear();

            // Общая информация
            StatsPanel.Children.Add(CreateSection("Общая информация"));
            StatsPanel.Children.Add(CreateInfoRow("Тип чата", chat.TypeChat + (chat.TypeGroup != null ? " • " + chat.TypeGroup : "")));
            StatsPanel.Children.Add(CreateInfoRow("Дата создания", chat.CreatedAt.ToString("dd.MM.yyyy HH:mm")));
            StatsPanel.Children.Add(CreateInfoRow("Владелец", GetOwnerName()));
            StatsPanel.Children.Add(CreateInfoRow("Статус", chat.IsArchived ? "📦 Архивирован" : "✅ Активен"));

            StatsPanel.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

            // Участники
            StatsPanel.Children.Add(CreateSection("Участники"));
            StatsPanel.Children.Add(CreateInfoRow("Всего участников", chat.Members.Count.ToString()));

            var owners = chat.Members.Count(m => m.Role == "Владелец");
            var admins = chat.Members.Count(m => m.Role == "Администратор");
            var members = chat.Members.Count(m => m.Role == "Участник");

            StatsPanel.Children.Add(CreateInfoRow("Владельцев", owners.ToString()));
            StatsPanel.Children.Add(CreateInfoRow("Администраторов", admins.ToString()));
            StatsPanel.Children.Add(CreateInfoRow("Участников", members.ToString()));

            StatsPanel.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

            // Сообщения
            StatsPanel.Children.Add(CreateSection("Сообщения"));
            StatsPanel.Children.Add(CreateInfoRow("Всего сообщений", chat.Messages.Count.ToString()));

            var myMessages = chat.Messages.Count(m => m.SenderId == App.CurrentUser.Id);
            var otherMessages = chat.Messages.Count - myMessages;
            StatsPanel.Children.Add(CreateInfoRow("Ваших сообщений", myMessages.ToString()));
            StatsPanel.Children.Add(CreateInfoRow("Сообщений других", otherMessages.ToString()));

            var pinnedCount = chat.Messages.Count(m => m.IsPinned);
            StatsPanel.Children.Add(CreateInfoRow("Закреплённых", pinnedCount.ToString()));

            var editedCount = chat.Messages.Count(m => m.EditTime.HasValue);
            StatsPanel.Children.Add(CreateInfoRow("Отредактированных", editedCount.ToString()));

            StatsPanel.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

            // Топ отправителей
            StatsPanel.Children.Add(CreateSection("Топ отправителей"));
            var topSenders = chat.Messages
                .GroupBy(m => m.SenderName)
                .OrderByDescending(g => g.Count())
                .Take(5);

            int rank = 1;
            foreach (var group in topSenders)
            {
                StatsPanel.Children.Add(CreateInfoRow($"#{rank} {group.Key}", $"{group.Count()} сообщ."));
                rank++;
            }

            // Задачи (если рабочий чат)
            if (chat.TypeGroup == "Рабочая")
            {
                StatsPanel.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });
                StatsPanel.Children.Add(CreateSection("Задачи"));
                StatsPanel.Children.Add(CreateInfoRow("Всего задач", chat.Tasks.Count.ToString()));

                var newTasks = chat.Tasks.Count(t => t.Status == "Новая");
                var inProgress = chat.Tasks.Count(t => t.Status == "В работе");
                var completed = chat.Tasks.Count(t => t.Status == "Выполнена");

                StatsPanel.Children.Add(CreateInfoRow("Новых", newTasks.ToString()));
                StatsPanel.Children.Add(CreateInfoRow("В работе", inProgress.ToString()));
                StatsPanel.Children.Add(CreateInfoRow("Выполнено", completed.ToString()));

                if (chat.Tasks.Count > 0)
                {
                    var completionPercent = (completed * 100 / chat.Tasks.Count);
                    StatsPanel.Children.Add(CreateInfoRow("Прогресс", $"{completionPercent}%"));
                }
            }
        }

        private string GetOwnerName()
        {
            var owner = chat.Members.FirstOrDefault(m => m.Role == "Владелец");
            if (owner != null)
            {
                var user = MockData.DataInitializer.Users.FirstOrDefault(u => u.Id == owner.UserId);
                return user?.FullName ?? "Неизвестно";
            }
            return "Неизвестно";
        }

        private TextBlock CreateSection(string title)
        {
            return new TextBlock
            {
                Text = title,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("PrimaryTextBrush"),
                Margin = new Thickness(0, 0, 0, 10)
            };
        }

        private Grid CreateInfoRow(string label, string value)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var labelText = new TextBlock
            {
                Text = label,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 13,
                Foreground = (Brush)FindResource("SecondaryTextBrush"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(labelText, 0);

            var valueText = new TextBlock
            {
                Text = value,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("PrimaryTextBrush"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(valueText, 1);

            grid.Children.Add(labelText);
            grid.Children.Add(valueText);

            return grid;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}