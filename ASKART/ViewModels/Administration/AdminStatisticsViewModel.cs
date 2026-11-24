using Askart.Services;
using System.Collections.ObjectModel;

namespace Askart.ViewModels.Administration;

public sealed record StatisticTile(string Caption, int Value, bool IsDanger = false);
public sealed record StatisticGroup(string Title, IReadOnlyList<StatisticTile> Tiles);
public sealed record ActivityItem(DateTime Date, int Count, string Filled, string Remaining);
public sealed record StatisticRow(string Label, string Value);

public sealed class AdminStatisticsViewModel : WindowViewModel
{
    private readonly AdminApiService api;
    public AdminNavigationViewModel Navigation { get; }
    public ObservableCollection<StatisticGroup> Groups { get; } = new();
    public ObservableCollection<ActivityItem> Activity { get; } = new();
    public ObservableCollection<StatisticRow> TopUsers { get; } = new();
    public ObservableCollection<StatisticRow> TopChats { get; } = new();
    public AsyncCommand Refresh { get; }

    public AdminStatisticsViewModel(AdminApiService api, AdminNavigationViewModel navigation)
    {
        this.api = api;
        Navigation = navigation;
        Refresh = new(_ => LoadAsync(), Error);
    }

    public async Task LoadAsync()
    {
        if (IsClosed) return;
        Groups.Clear();
        Activity.Clear();
        TopUsers.Clear();
        TopChats.Clear();
        Status = "Загрузка статистики…";
        try
        {
            var stats = await api.GetStatsAsync(Lifetime);
            if (IsClosed) return;
            Groups.Clear();
            Groups.Add(new("Пользователи", new StatisticTile[] {
                new("Всего", stats.Users.Total), new("Новых за 7 дней", stats.Users.New7Days),
                new("Новых за 30 дней", stats.Users.New30Days), new("Активных за 7 дней", stats.Users.Active7Days),
                new("Администраторов", stats.Users.Admins) }));
            Groups.Add(new("Чаты", new StatisticTile[] {
                new("Всего", stats.Chats.Total), new("Личных", stats.Chats.PrivateChats),
                new("Личных групп", stats.Chats.PersonalGroups), new("Рабочих групп", stats.Chats.WorkGroups) }));
            Groups.Add(new("Сообщения", new StatisticTile[] {
                new("Всего", stats.Messages.Total), new("Сегодня", stats.Messages.Today),
                new("За 7 дней", stats.Messages.Last7Days), new("Отредактировано", stats.Messages.Edited) }));
            Groups.Add(new("Задачи", new StatisticTile[] {
                new("Всего", stats.Tasks.Total), new("Новых", stats.Tasks.Created),
                new("В работе", stats.Tasks.InProgress), new("Выполнено", stats.Tasks.Completed),
                new("Просрочено", stats.Tasks.Overdue, stats.Tasks.Overdue > 0) }));
            Activity.Clear();
            int max = Math.Max(1, stats.MessagesPerDay.Select(day => day.Count).DefaultIfEmpty().Max());
            foreach (var day in stats.MessagesPerDay)
                Activity.Add(new(day.Date, day.Count, day.Count + "*", (max - day.Count) + "*"));
            ReplaceTop(TopUsers, stats.TopUsers);
            ReplaceTop(TopChats, stats.TopChats);
            Status = "";
        }
        catch (OperationCanceledException) when (IsClosed) { }
        catch (Exception ex) { Error(ex); }
    }

    private static void ReplaceTop(ObservableCollection<StatisticRow> rows, List<NameCount> values)
    {
        rows.Clear();
        if (values.Count == 0) rows.Add(new("Пока нет данных", ""));
        for (int i = 0; i < values.Count; i++)
            rows.Add(new("#" + (i + 1) + " " + values[i].Name, values[i].Count + " сообщ."));
    }
}
