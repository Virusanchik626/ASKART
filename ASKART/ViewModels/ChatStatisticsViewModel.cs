using Askart.Models;
using Askart.Services;
using System.Collections.ObjectModel;

namespace Askart.ViewModels;

public sealed record ChatStatisticRow(string Kind, string Label, string Value)
{
    public static ChatStatisticRow Section(string title) => new("Section", title, "");
    public static ChatStatisticRow Info(string label, string value) => new("Info", label, value);
    public static ChatStatisticRow Separator() => new("Separator", "", "");
}

public sealed class ChatStatisticsViewModel : WindowViewModel
{
    private readonly Chat chat;
    private readonly ChatService chats;
    private readonly MessageService messages;
    private readonly TaskService tasks;
    private readonly UserDirectoryService users;
    private readonly int userId;
    public string ChatName => chat.Name;
    public ObservableCollection<ChatStatisticRow> Rows { get; } = new();

    public ChatStatisticsViewModel(Chat chat, int userId, ChatService chats,
        MessageService messages, TaskService tasks, UserDirectoryService users)
    {
        this.chat = chat;
        this.userId = userId;
        this.chats = chats;
        this.messages = messages;
        this.tasks = tasks;
        this.users = users;
    }

    public async Task LoadAsync()
    {
        if (IsClosed) return;
        try
        {
            var members = await chats.GetMembers(chat, Lifetime);
            var owner = members.FirstOrDefault(member => member.Role == "Владелец");
            var ownerUser = owner == null ? null : await users.GetUserById(owner.UserId, Lifetime);
            var allMessages = await messages.GetMessages(chat, Lifetime);
            var rows = new List<ChatStatisticRow>
            {
                ChatStatisticRow.Section("Общая информация"),
                ChatStatisticRow.Info("Тип чата", chat.TypeChat + (chat.TypeGroup == null ? "" : " • " + chat.TypeGroup)),
                ChatStatisticRow.Info("Дата создания", chat.CreatedAt.ToString("dd.MM.yyyy HH:mm")),
                ChatStatisticRow.Info("Владелец", ownerUser?.FullName ?? "Неизвестно"),
                ChatStatisticRow.Info("Статус", chat.IsArchived ? "📦 Архивирован" : "✅ Активен"),
                ChatStatisticRow.Separator(),
                ChatStatisticRow.Section("Участники"),
                ChatStatisticRow.Info("Всего участников", members.Count.ToString()),
                ChatStatisticRow.Info("Владельцев", members.Count(member => member.Role == "Владелец").ToString()),
                ChatStatisticRow.Info("Администраторов", members.Count(member => member.Role == "Администратор").ToString()),
                ChatStatisticRow.Info("Участников", members.Count(member => member.Role == "Участник").ToString()),
                ChatStatisticRow.Separator(),
                ChatStatisticRow.Section("Сообщения"),
                ChatStatisticRow.Info("Всего сообщений", allMessages.Count.ToString()),
                ChatStatisticRow.Info("Ваших сообщений", allMessages.Count(message => message.SenderId == userId).ToString()),
                ChatStatisticRow.Info("Сообщений других", allMessages.Count(message => message.SenderId != userId).ToString()),
                ChatStatisticRow.Info("Закреплённых", allMessages.Count(message => message.IsPinned).ToString()),
                ChatStatisticRow.Info("Отредактированных", allMessages.Count(message => message.EditTime.HasValue).ToString()),
                ChatStatisticRow.Separator(),
                ChatStatisticRow.Section("Топ отправителей")
            };
            var top = allMessages.GroupBy(message => message.SenderName)
                .OrderByDescending(group => group.Count()).Take(5).ToList();
            for (int i = 0; i < top.Count; i++)
                rows.Add(ChatStatisticRow.Info("#" + (i + 1) + " " + top[i].Key, top[i].Count() + " сообщ."));
            if (chat.TypeGroup == "Рабочая")
            {
                var allTasks = await tasks.GetTasks(chat, Lifetime);
                int completed = allTasks.Count(task => task.Status == "Выполнена");
                rows.Add(ChatStatisticRow.Separator());
                rows.Add(ChatStatisticRow.Section("Задачи"));
                rows.Add(ChatStatisticRow.Info("Всего задач", allTasks.Count.ToString()));
                rows.Add(ChatStatisticRow.Info("Новых", allTasks.Count(task => task.Status == "Новая").ToString()));
                rows.Add(ChatStatisticRow.Info("В работе", allTasks.Count(task => task.Status == "В работе").ToString()));
                rows.Add(ChatStatisticRow.Info("Выполнено", completed.ToString()));
                if (allTasks.Count > 0)
                    rows.Add(ChatStatisticRow.Info("Прогресс", completed * 100 / allTasks.Count + "%"));
            }
            if (IsClosed) return;
            Rows.Clear();
            foreach (var row in rows) Rows.Add(row);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Error(ex); }
    }
}
