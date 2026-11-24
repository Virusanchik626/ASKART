using Askart.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
namespace Askart.Services;

public sealed class TaskService
{
    private readonly ApiTransport api;
    private readonly UserDirectoryService users;
    public TaskService(ApiTransport api, UserDirectoryService users)
    {
        this.api = api;
        this.users = users;
    }
        public async Task<List<TaskItem>> GetTasks(Chat chat, CancellationToken cancellationToken = default)
        {
            string json = await api.SendAsync(HttpMethod.Get, "Tasks/chat/" + chat.Id, null, cancellationToken);
            if (json != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                List<ApiTask> list = JsonSerializer.Deserialize<List<ApiTask>>(json, ApiTransport.JsonOptions);

                chat.Tasks.Clear();
                foreach (ApiTask a in list)
                {
                    chat.Tasks.Add(await MakeTask(a, cancellationToken));
                }
            }
            else
            {
                api.NotifyError();
            }

            List<TaskItem> result = new List<TaskItem>();
            foreach (TaskItem task in chat.Tasks)
            {
                result.Add(task);
            }
            return result;
        }

        private async Task<TaskItem> MakeTask(ApiTask a, CancellationToken cancellationToken = default)
        {
            TaskItem task = new TaskItem();
            task.Id = a.TaskId;
            task.ChatId = a.ChatId;
            task.Name = a.Name;
            task.Description = a.Describe;
            if (a.Describe == "-")
            {
                task.Description = "";
            }
            task.Status = a.Status;
            task.Priority = a.Priority;
            task.Deadline = a.Deadline;
            task.CreatorId = a.CreatorId;
            task.AssigneeUserId = a.AssigneUserId;
            task.AssigneeName = "";

            User assignee = await users.GetUserById(a.AssigneUserId, cancellationToken);
            if (assignee != null)
            {
                task.AssigneeName = assignee.FullName;
            }
            return task;
        }

        public async Task<TaskItem> AddTask(Chat chat, string name, string description, string status,
                                       DateTime deadline, string priority, User creator, CancellationToken cancellationToken = default)
        {
            string desc = description;
            if (string.IsNullOrWhiteSpace(desc))
            {
                desc = "-";
            }

            string json = JsonSerializer.Serialize(new
            {
                chatId = chat.Id,
                name = name,
                describe = desc,
                creatorId = creator.Id,
                assigneUserId = creator.Id,
                deadline = deadline,
                priority = priority
            });

            string answer = await api.SendAsync(HttpMethod.Post, "Tasks", json, cancellationToken);
            if (answer == null)
            {
                api.NotifyError();
                return null;
            }

            ApiTask a = JsonSerializer.Deserialize<ApiTask>(answer, ApiTransport.JsonOptions);
            TaskItem task = await MakeTask(a, cancellationToken);
            task.AssigneeName = creator.FullName;
            chat.Tasks.Add(task);
            if (status != "Новая")
            {
                await SetTaskStatus(task, status, cancellationToken);
            }

            return task;
        }

        public async Task DeleteTask(Chat chat, TaskItem task, CancellationToken cancellationToken = default)
        {
            string answer = await api.SendAsync(HttpMethod.Delete, "Tasks/" + task.Id, null, cancellationToken);
            if (answer == null)
            {
                api.NotifyError();
                return;
            }

            chat.Tasks.Remove(task);
        }

        public async Task SetTaskStatus(TaskItem task, string newStatus, CancellationToken cancellationToken = default)
        {
            string desc = task.Description;
            if (string.IsNullOrWhiteSpace(desc))
            {
                desc = "-";
            }

            string json = JsonSerializer.Serialize(new
            {
                name = task.Name,
                describe = desc,
                status = newStatus,
                priority = task.Priority,
                deadline = task.Deadline
            });

            string answer = await api.SendAsync(HttpMethod.Put, "Tasks/" + task.Id, json, cancellationToken);
            if (answer == null)
            {
                api.NotifyError();
                return;
            }

            task.Status = newStatus;
        }

}
