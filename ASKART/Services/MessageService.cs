using Askart.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
namespace Askart.Services;

public sealed class MessageService
{
    private readonly ApiTransport api;
    private readonly List<int> pinnedMessageIds = new();
    public MessageService(ApiTransport api) { this.api = api; }
    public void ClearCache() => pinnedMessageIds.Clear();
        public async Task<List<Message>> GetMessages(Chat chat, CancellationToken cancellationToken = default)
        {
            string json = await api.SendAsync(HttpMethod.Get, "Messages/chat/" + chat.Id, null, cancellationToken);
            if (json != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                List<ApiMessage> list = JsonSerializer.Deserialize<List<ApiMessage>>(json, ApiTransport.JsonOptions);

                chat.Messages.Clear();
                foreach (ApiMessage a in list)
                {
                    chat.Messages.Add(MakeMessage(a));
                }

                foreach (Message msg in chat.Messages)
                {
                    if (msg.ReplyId.HasValue)
                    {
                        msg.ReplyTo = GetMessageById(chat, msg.ReplyId.Value);
                    }
                }
            }
            else
            {
                api.NotifyError();
            }

            List<Message> result = new List<Message>();
            foreach (Message message in chat.Messages)
            {
                result.Add(message);
            }
            return result;
        }

        private Message MakeMessage(ApiMessage a)
        {
            Message message = new Message();
            message.Id = a.MessageId;
            message.Text = a.Text;
            message.ChatId = a.ChatId;
            message.SenderId = a.SenderId;
            message.SenderName = a.SenderName;
            message.ReplyId = a.ReplyId;
            message.SendTime = a.SendTime;
            message.EditTime = a.EditTime;
            message.IsPinned = pinnedMessageIds.Contains(a.MessageId);
            return message;
        }

        public async Task<List<Message>> GetPinnedMessages(Chat chat, CancellationToken cancellationToken = default)
        {
            await GetMessages(chat, cancellationToken);
            List<Message> result = new List<Message>();
            foreach (Message message in chat.Messages)
            {
                if (message.IsPinned)
                {
                    result.Insert(0, message);
                }
            }
            return result;
        }

        public Message GetMessageById(Chat chat, int messageId)
        {
            foreach (Message message in chat.Messages)
            {
                if (message.Id == messageId)
                {
                    return message;
                }
            }
            return null;
        }
        public async Task<Message> SendMessage(Chat chat, User sender, string text, Message replyTo, CancellationToken cancellationToken = default)
        {
            int? replyId = null;
            if (replyTo != null)
            {
                replyId = replyTo.Id;
            }

            string json = JsonSerializer.Serialize(new
            {
                chatId = chat.Id,
                senderId = sender.Id,
                text = text,
                replyId = replyId
            });

            string answer = await api.SendAsync(HttpMethod.Post, "Messages", json, cancellationToken);
            if (answer == null)
            {
                api.NotifyError();
                return null;
            }

            ApiMessage a = JsonSerializer.Deserialize<ApiMessage>(answer, ApiTransport.JsonOptions);
            Message message = MakeMessage(a);
            message.ReplyTo = replyTo;

            chat.Messages.Add(message);
            return message;
        }

        public async Task EditMessage(Message message, string newText, CancellationToken cancellationToken = default)
        {
            string json = JsonSerializer.Serialize(newText);
            string answer = await api.SendAsync(HttpMethod.Put, "Messages/" + message.Id, json, cancellationToken);
            if (answer == null)
            {
                api.NotifyError();
                return;
            }

            message.Text = newText;
            message.EditTime = DateTime.Now;
        }

        public async Task DeleteMessage(Chat chat, Message message, CancellationToken cancellationToken = default)
        {
            string answer = await api.SendAsync(HttpMethod.Delete, "Messages/" + message.Id, null, cancellationToken);
            if (answer == null)
            {
                api.NotifyError();
                return;
            }

            chat.Messages.Remove(message);
        }

        public void SetPinned(Message message, bool isPinned)
        {
            message.IsPinned = isPinned;

            if (isPinned)
            {
                if (pinnedMessageIds.Contains(message.Id) == false)
                {
                    pinnedMessageIds.Add(message.Id);
                }
            }
            else
            {
                pinnedMessageIds.Remove(message.Id);
            }
        }

}
