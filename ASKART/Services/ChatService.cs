using Askart.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
namespace Askart.Services;

public sealed class ChatService
{
    private readonly ApiTransport api;
    private readonly UserDirectoryService users;
    private readonly List<int> archivedChatIds = new();
    private List<Chat> myChats = new();
    private bool chatsLoaded;
    private int myChatsUserId;
    public ChatService(ApiTransport api, UserDirectoryService users)
    {
        this.api = api;
        this.users = users;
    }
    public void ClearCache()
    {
        archivedChatIds.Clear();
        myChats.Clear();
        chatsLoaded = false;
        myChatsUserId = 0;
    }
        public async Task<List<Chat>> GetMyChats(int userId, bool showArchived, string searchText, CancellationToken cancellationToken = default)
        {
            string search = searchText.ToLower();

            if (chatsLoaded == false || myChatsUserId != userId || search == "")
            {
                await LoadMyChats(userId, cancellationToken);
            }

            List<Chat> result = new List<Chat>();
            foreach (Chat chat in myChats)
            {
                if (chat.IsArchived && showArchived == false)
                {
                    continue;
                }

                if (search != "" && chat.Name.ToLower().Contains(search) == false)
                {
                    continue;
                }

                result.Add(chat);
            }
            return result;
        }

        private async Task LoadMyChats(int userId, CancellationToken cancellationToken = default)
        {
            string json = await api.SendAsync(HttpMethod.Get, "Chats/user/" + userId, null, cancellationToken);
            if (json == null)
            {
                api.NotifyError();
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            List<ApiChat> list = JsonSerializer.Deserialize<List<ApiChat>>(json, ApiTransport.JsonOptions);

            var loaded = new List<Chat>();
            foreach (ApiChat a in list)
            {
                Chat chat = MakeChat(a);
                await LoadMembers(chat, cancellationToken);
                loaded.Add(chat);
            }

            cancellationToken.ThrowIfCancellationRequested();
            myChats = loaded;
            chatsLoaded = true;
            myChatsUserId = userId;
        }

        private Chat MakeChat(ApiChat a)
        {
            Chat chat = new Chat();
            chat.Id = a.ChatId;
            chat.Name = a.Name;
            chat.TypeChat = a.TypeChat;
            chat.TypeGroup = a.TypeGroup;
            chat.IsArchived = archivedChatIds.Contains(a.ChatId);
            chat.KanbanColumns.Clear();
            return chat;
        }

        public async Task<Chat> CreateChat(string name, bool isGroup, bool isWork, User owner, CancellationToken cancellationToken = default)
        {
            if (isGroup == false)
            {
                await users.LoadUsers(cancellationToken);
                User other = null;
                foreach (User user in users.allUsers)
                {
                    if (user.Login == name || user.Email == name)
                    {
                        other = user;
                    }
                }

                if (other == null)
                {
                    api.LastError = "Пользователь с таким логином или почтой не найден";
                    api.NotifyError();
                    return null;
                }

                string jsonPrivate = JsonSerializer.Serialize(new { userId = owner.Id, otherUserId = other.Id });
                string answerPrivate = await api.SendAsync(HttpMethod.Post, "Chats/private", jsonPrivate, cancellationToken);
                if (answerPrivate == null)
                {
                    api.NotifyError();
                    return null;
                }

                ApiChat privateChat = JsonSerializer.Deserialize<ApiChat>(answerPrivate, ApiTransport.JsonOptions);
                privateChat.TypeChat = "Личный";
                privateChat.TypeGroup = null;
                if (string.IsNullOrEmpty(privateChat.Name))
                {
                    privateChat.Name = other.Surname + " " + other.Name;
                }

                Chat newPrivate = MakeChat(privateChat);
                await LoadMembers(newPrivate, cancellationToken);
                return newPrivate;
            }

            string typeGroup = "Личная";
            if (isWork)
            {
                typeGroup = "Рабочая";
            }

            string jsonGroup = JsonSerializer.Serialize(new { name = name, creatorId = owner.Id, typeGroup = typeGroup });
            string answerGroup = await api.SendAsync(HttpMethod.Post, "Chats/group", jsonGroup, cancellationToken);
            if (answerGroup == null)
            {
                api.NotifyError();
                return null;
            }

            ApiChat groupChat = JsonSerializer.Deserialize<ApiChat>(answerGroup, ApiTransport.JsonOptions);
            Chat newGroup = MakeChat(groupChat);
            await LoadMembers(newGroup, cancellationToken);
            return newGroup;
        }

        public void SetArchived(Chat chat, bool isArchived)
        {
            chat.IsArchived = isArchived;

            if (isArchived)
            {
                if (archivedChatIds.Contains(chat.Id) == false)
                {
                    archivedChatIds.Add(chat.Id);
                }
            }
            else
            {
                archivedChatIds.Remove(chat.Id);
            }
        }

        public async Task<List<ChatMember>> GetMembers(Chat chat, CancellationToken cancellationToken = default)
        {
            await LoadMembers(chat, cancellationToken);

            List<ChatMember> result = new List<ChatMember>();
            foreach (ChatMember member in chat.Members)
            {
                result.Add(member);
            }
            return result;
        }

        private async Task LoadMembers(Chat chat, CancellationToken cancellationToken = default)
        {
            string json = await api.SendAsync(HttpMethod.Get, "ChatMembers/" + chat.Id, null, cancellationToken);
            if (json == null)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            List<ApiMember> list = JsonSerializer.Deserialize<List<ApiMember>>(json, ApiTransport.JsonOptions);

            chat.Members.Clear();
            foreach (ApiMember a in list)
            {
                ChatMember member = new ChatMember();
                member.ChatId = chat.Id;
                member.UserId = a.UserId;

                member.Role = a.Role;
                if (a.Role == "Создатель")
                {
                    member.Role = "Владелец";
                }

                if (member.Role == "Владелец")
                {
                    chat.OwnerId = a.UserId;
                }

                chat.Members.Add(member);
            }
        }


}
