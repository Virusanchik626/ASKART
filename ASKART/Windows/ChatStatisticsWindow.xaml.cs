using Askart.Models;
using Askart.Services;
using System.Windows;
using System.Windows.Controls;

namespace Askart.Windows
{
    public partial class ChatStatisticsWindow : Window
    {
        private Chat chat;

        public ChatStatisticsWindow(Chat chat)
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
            this.chat = chat;
            ChatNameText.Text = chat.Name;
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            StatsPanel.Children.Clear();

            
            StatsPanel.Children.Add(CreateSection("Общая информация"));

            string typeText = chat.TypeChat;
            if (chat.TypeGroup != null)
            {
                typeText = typeText + " • " + chat.TypeGroup;
            }
            StatsPanel.Children.Add(CreateInfoRow("Тип чата", typeText));
            StatsPanel.Children.Add(CreateInfoRow("Дата создания", chat.CreatedAt.ToString("dd.MM.yyyy HH:mm")));
            StatsPanel.Children.Add(CreateInfoRow("Владелец", GetOwnerName()));

            if (chat.IsArchived)
            {
                StatsPanel.Children.Add(CreateInfoRow("Статус", "📦 Архивирован"));
            }
            else
            {
                StatsPanel.Children.Add(CreateInfoRow("Статус", "✅ Активен"));
            }

            StatsPanel.Children.Add(CreateSeparator());

            
            List<ChatMember> members = DataService.GetMembers(chat);

            int owners = 0;
            int admins = 0;
            int simpleMembers = 0;
            foreach (ChatMember member in members)
            {
                if (member.Role == "Владелец")
                {
                    owners++;
                }
                else if (member.Role == "Администратор")
                {
                    admins++;
                }
                else if (member.Role == "Участник")
                {
                    simpleMembers++;
                }
            }

            StatsPanel.Children.Add(CreateSection("Участники"));
            StatsPanel.Children.Add(CreateInfoRow("Всего участников", members.Count.ToString()));
            StatsPanel.Children.Add(CreateInfoRow("Владельцев", owners.ToString()));
            StatsPanel.Children.Add(CreateInfoRow("Администраторов", admins.ToString()));
            StatsPanel.Children.Add(CreateInfoRow("Участников", simpleMembers.ToString()));

            StatsPanel.Children.Add(CreateSeparator());

            
            List<Message> messages = DataService.GetMessages(chat);

            int myMessages = 0;
            int pinnedCount = 0;
            int editedCount = 0;
            foreach (Message message in messages)
            {
                if (message.SenderId == App.CurrentUser.Id)
                {
                    myMessages++;
                }
                if (message.IsPinned)
                {
                    pinnedCount++;
                }
                if (message.EditTime.HasValue)
                {
                    editedCount++;
                }
            }
            int otherMessages = messages.Count - myMessages;

            StatsPanel.Children.Add(CreateSection("Сообщения"));
            StatsPanel.Children.Add(CreateInfoRow("Всего сообщений", messages.Count.ToString()));
            StatsPanel.Children.Add(CreateInfoRow("Ваших сообщений", myMessages.ToString()));
            StatsPanel.Children.Add(CreateInfoRow("Сообщений других", otherMessages.ToString()));
            StatsPanel.Children.Add(CreateInfoRow("Закреплённых", pinnedCount.ToString()));
            StatsPanel.Children.Add(CreateInfoRow("Отредактированных", editedCount.ToString()));

            StatsPanel.Children.Add(CreateSeparator());

            
            StatsPanel.Children.Add(CreateSection("Топ отправителей"));
            ShowTopSenders(messages);

            
            if (chat.TypeGroup == "Рабочая")
            {
                List<TaskItem> tasks = DataService.GetTasks(chat);

                int newTasks = 0;
                int inProgress = 0;
                int completed = 0;
                foreach (TaskItem task in tasks)
                {
                    if (task.Status == "Новая")
                    {
                        newTasks++;
                    }
                    else if (task.Status == "В работе")
                    {
                        inProgress++;
                    }
                    else if (task.Status == "Выполнена")
                    {
                        completed++;
                    }
                }

                StatsPanel.Children.Add(CreateSeparator());
                StatsPanel.Children.Add(CreateSection("Задачи"));
                StatsPanel.Children.Add(CreateInfoRow("Всего задач", tasks.Count.ToString()));
                StatsPanel.Children.Add(CreateInfoRow("Новых", newTasks.ToString()));
                StatsPanel.Children.Add(CreateInfoRow("В работе", inProgress.ToString()));
                StatsPanel.Children.Add(CreateInfoRow("Выполнено", completed.ToString()));

                if (tasks.Count > 0)
                {
                    int completionPercent = completed * 100 / tasks.Count;
                    StatsPanel.Children.Add(CreateInfoRow("Прогресс", completionPercent + "%"));
                }
            }
        }

        
        private void ShowTopSenders(List<Message> messages)
        {
            List<string> names = new List<string>();
            List<int> counts = new List<int>();

            foreach (Message message in messages)
            {
                int index = names.IndexOf(message.SenderName);
                if (index == -1)
                {
                    names.Add(message.SenderName);
                    counts.Add(1);
                }
                else
                {
                    counts[index] = counts[index] + 1;
                }
            }

            
            for (int i = 0; i < names.Count - 1; i++)
            {
                for (int j = 0; j < names.Count - 1 - i; j++)
                {
                    if (counts[j] < counts[j + 1])
                    {
                        int tempCount = counts[j];
                        counts[j] = counts[j + 1];
                        counts[j + 1] = tempCount;

                        string tempName = names[j];
                        names[j] = names[j + 1];
                        names[j + 1] = tempName;
                    }
                }
            }

            for (int i = 0; i < names.Count && i < 5; i++)
            {
                int rank = i + 1;
                StatsPanel.Children.Add(CreateInfoRow("#" + rank + " " + names[i], counts[i] + " сообщ."));
            }
        }

        private string GetOwnerName()
        {
            List<ChatMember> members = DataService.GetMembers(chat);
            foreach (ChatMember member in members)
            {
                if (member.Role == "Владелец")
                {
                    User user = DataService.GetUserById(member.UserId);
                    if (user != null)
                    {
                        return user.FullName;
                    }
                }
            }
            return "Неизвестно";
        }

        private Separator CreateSeparator()
        {
            Separator separator = new Separator();
            separator.Margin = new Thickness(0, 15, 0, 15);
            return separator;
        }

        private TextBlock CreateSection(string title)
        {
            TextBlock textBlock = UiHelper.MakeText(title, 16, "PrimaryTextBrush");
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.Margin = new Thickness(0, 0, 0, 10);
            return textBlock;
        }

        private Grid CreateInfoRow(string label, string value)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock labelText = UiHelper.MakeText(label, 13, "SecondaryTextBrush");
            labelText.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(labelText, 0);

            TextBlock valueText = UiHelper.MakeText(value, 13, "PrimaryTextBrush");
            valueText.FontWeight = FontWeights.SemiBold;
            valueText.VerticalAlignment = VerticalAlignment.Center;
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
