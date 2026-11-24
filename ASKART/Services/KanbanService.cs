using Askart.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
namespace Askart.Services;

public sealed class KanbanService
{
    private readonly ApiTransport api;
    public KanbanService(ApiTransport api) { this.api = api; }
        public async Task<List<KanbanColumn>> GetColumns(Chat chat, CancellationToken cancellationToken = default)
        {
            string json = await api.SendAsync(HttpMethod.Get, "Kanban/chat/" + chat.Id, null, cancellationToken);
            if (json != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                List<ApiColumn> list = JsonSerializer.Deserialize<List<ApiColumn>>(json, ApiTransport.JsonOptions);

                if (list.Count == 0)
                {
                    await api.SendAsync(HttpMethod.Post, "Kanban/chat/" + chat.Id + "/initialize", null, cancellationToken);
                    json = await api.SendAsync(HttpMethod.Get, "Kanban/chat/" + chat.Id, null, cancellationToken);
                    if (json != null)
                    {
                        list = JsonSerializer.Deserialize<List<ApiColumn>>(json, ApiTransport.JsonOptions);
                    }
                }

                chat.KanbanColumns.Clear();
                foreach (ApiColumn a in list)
                {
                    KanbanColumn column = new KanbanColumn();
                    column.Id = a.ColumnId;
                    column.Name = a.Name;
                    column.Color = a.Color;
                    chat.KanbanColumns.Add(column);
                }
            }
            else
            {
                api.NotifyError();
            }

            List<KanbanColumn> result = new List<KanbanColumn>();
            foreach (KanbanColumn column in chat.KanbanColumns)
            {
                result.Add(column);
            }
            return result;
        }

        public string AddColumn(Chat chat, string name)
        {
            return "Апишка пока не умеет создавать новые колонки (в базе только: Новая, В работе, Выполнена)";
        }

        public void DeleteColumn(Chat chat, KanbanColumn column)
        {
            api.LastError = "Апишка пока не умеет удалять колонки";
            api.NotifyError();
        }

        public async Task MoveColumn(Chat chat, KanbanColumn column, int direction, CancellationToken cancellationToken = default)
        {
            int oldIndex = chat.KanbanColumns.IndexOf(column);
            int newIndex = oldIndex + direction;

            if (oldIndex < 0 || newIndex < 0 || newIndex >= chat.KanbanColumns.Count)
            {
                return;
            }

            KanbanColumn other = chat.KanbanColumns[newIndex];

            bool ok1 = await SaveColumn(chat, column, column.Color, newIndex + 1, cancellationToken);
            bool ok2 = await SaveColumn(chat, other, other.Color, oldIndex + 1, cancellationToken);

            if (ok1 && ok2)
            {
                chat.KanbanColumns.RemoveAt(oldIndex);
                chat.KanbanColumns.Insert(newIndex, column);
            }
        }

        public async Task<string> UpdateColumn(Chat chat, KanbanColumn column, string newName, string newColor, CancellationToken cancellationToken = default)
        {
            if (newName != column.Name)
            {
                return "Апишка пока не умеет переименовывать колонки, можно менять только цвет";
            }

            int position = chat.KanbanColumns.IndexOf(column) + 1;
            bool ok = await SaveColumn(chat, column, newColor, position, cancellationToken);
            if (ok == false)
            {
                return api.LastError;
            }

            column.Color = newColor;
            return null;
        }

        private async Task<bool> SaveColumn(Chat chat, KanbanColumn column, string color, int position, CancellationToken cancellationToken = default)
        {
            string json = JsonSerializer.Serialize(new { color = color, position = position });
            string answer = await api.SendAsync(HttpMethod.Put, "Kanban/" + chat.Id + "/" + column.Id, json, cancellationToken);
            if (answer == null)
            {
                api.NotifyError();
                return false;
            }
            return true;
        }
}
