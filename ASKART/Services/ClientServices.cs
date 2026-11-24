using System.Net.Http;

namespace Askart.Services;

public sealed class ClientServices
{
    public ApiTransport Transport { get; }
    public UserDirectoryService Users { get; }
    public ChatService Chats { get; }
    public MessageService Messages { get; }
    public TaskService Tasks { get; }
    public KanbanService Kanban { get; }

    public ClientServices()
    {
        Transport = new(new HttpClient { Timeout = TimeSpan.FromSeconds(10) });
        Users = new(Transport);
        Chats = new(Transport, Users);
        Messages = new(Transport);
        Tasks = new(Transport, Users);
        Kanban = new(Transport);
    }

    public void ClearSessionCache()
    {
        Transport.ResetSession();
        Users.ClearCache();
        Chats.ClearCache();
        Messages.ClearCache();
        Transport.LastError = "";
    }
}
