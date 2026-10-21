using Askart.MockData;
using Askart.Models;

namespace Askart.Services
{
    
    
    
    
    
    
    
    
    
    
    public static class DataService
    {
        

        
        public static User Login(string email, string password)
        {
            
            foreach (User user in DataInitializer.Users)
            {
                if (user.Login == email && user.Password == password)
                {
                    return user;
                }
            }
            return null;
        }

        
        public static string Register(string name, string surname, string email, string password, string phone)
        {
            
            foreach (User user in DataInitializer.Users)
            {
                if (user.Login == email || user.Email == email)
                {
                    return "Пользователь с такой почтой уже существует";
                }
            }

            int newId = 1;
            foreach (User user in DataInitializer.Users)
            {
                if (user.Id >= newId)
                {
                    newId = user.Id + 1;
                }
            }

            User newUser = new User();
            newUser.Id = newId;
            newUser.Name = name;
            newUser.Surname = surname;
            newUser.Lastname = "";
            newUser.Login = email;
            newUser.Password = password;
            newUser.Email = email;
            newUser.Number = phone;
            newUser.RegistrationDate = DateTime.Now;

            DataInitializer.Users.Add(newUser);
            return null;
        }

        
        public static User GetUserById(int userId)
        {
            
            foreach (User user in DataInitializer.Users)
            {
                if (user.Id == userId)
                {
                    return user;
                }
            }
            return null;
        }

        

        
        public static List<Chat> GetMyChats(int userId, bool showArchived, string searchText)
        {
            
            List<Chat> result = new List<Chat>();
            string search = searchText.ToLower();

            foreach (Chat chat in DataInitializer.Chats)
            {
                bool isMember = false;
                foreach (ChatMember member in chat.Members)
                {
                    if (member.UserId == userId)
                    {
                        isMember = true;
                    }
                }
                if (isMember == false)
                {
                    continue;
                }

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

        public static Chat CreateChat(string name, bool isGroup, bool isWork, User owner)
        {
            
            int newId = 1;
            foreach (Chat c in DataInitializer.Chats)
            {
                if (c.Id >= newId)
                {
                    newId = c.Id + 1;
                }
            }

            Chat newChat = new Chat();
            newChat.Id = newId;
            newChat.Name = name;
            newChat.CreatedAt = DateTime.Now;
            newChat.OwnerId = owner.Id;

            if (isGroup)
            {
                newChat.TypeChat = "Группа";
                if (isWork)
                {
                    newChat.TypeGroup = "Рабочая";
                }
                else
                {
                    newChat.TypeGroup = "Личная";
                }
            }
            else
            {
                newChat.TypeChat = "Личный";
                newChat.TypeGroup = null;
            }

            ChatMember me = new ChatMember();
            me.ChatId = newId;
            me.UserId = owner.Id;
            me.Role = "Владелец";
            newChat.Members.Add(me);

            DataInitializer.Chats.Add(newChat);
            return newChat;
        }

        public static void SetArchived(Chat chat, bool isArchived)
        {
            
            chat.IsArchived = isArchived;
        }

        public static List<ChatMember> GetMembers(Chat chat)
        {
            
            List<ChatMember> result = new List<ChatMember>();
            foreach (ChatMember member in chat.Members)
            {
                result.Add(member);
            }
            return result;
        }

        

        public static List<Message> GetMessages(Chat chat)
        {
            
            List<Message> result = new List<Message>();
            foreach (Message message in chat.Messages)
            {
                result.Add(message);
            }
            return result;
        }

        
        public static List<Message> GetPinnedMessages(Chat chat)
        {
            
            
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

        
        public static Message GetMessageById(Chat chat, int messageId)
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

        
        public static Message SendMessage(Chat chat, User sender, string text, Message replyTo)
        {
            
            int newId = 1;
            foreach (Message m in chat.Messages)
            {
                if (m.Id >= newId)
                {
                    newId = m.Id + 1;
                }
            }

            Message message = new Message();
            message.Id = newId;
            message.Text = text;
            message.ChatId = chat.Id;
            message.SenderId = sender.Id;
            message.SenderName = sender.FullName;
            message.SendTime = DateTime.Now;
            if (replyTo != null)
            {
                message.ReplyId = replyTo.Id;
                message.ReplyTo = replyTo;
            }

            chat.Messages.Add(message);
            return message;
        }

        public static void EditMessage(Message message, string newText)
        {
            
            message.Text = newText;
            message.EditTime = DateTime.Now;
        }

        public static void DeleteMessage(Chat chat, Message message)
        {
            
            chat.Messages.Remove(message);
        }

        public static void SetPinned(Message message, bool isPinned)
        {
            
            message.IsPinned = isPinned;
        }

        

        public static List<TaskItem> GetTasks(Chat chat)
        {
            
            List<TaskItem> result = new List<TaskItem>();
            foreach (TaskItem task in chat.Tasks)
            {
                result.Add(task);
            }
            return result;
        }

        public static TaskItem AddTask(Chat chat, string name, string description, string status,
                                       DateTime deadline, string priority, User creator)
        {
            
            int newId = 1;
            foreach (TaskItem t in chat.Tasks)
            {
                if (t.Id >= newId)
                {
                    newId = t.Id + 1;
                }
            }

            TaskItem task = new TaskItem();
            task.Id = newId;
            task.ChatId = chat.Id;
            task.Name = name;
            task.Description = description;
            task.Status = status;
            task.Deadline = deadline;
            task.Priority = priority;
            task.CreatorId = creator.Id;
            task.AssigneeUserId = creator.Id;
            task.AssigneeName = creator.FullName;
            task.CreatedAt = DateTime.Now;

            chat.Tasks.Add(task);
            return task;
        }

        public static void DeleteTask(Chat chat, TaskItem task)
        {
            
            chat.Tasks.Remove(task);
        }

        
        public static void SetTaskStatus(TaskItem task, string newStatus)
        {
            
            task.Status = newStatus;
        }

        

        public static List<KanbanColumn> GetColumns(Chat chat)
        {
            
            List<KanbanColumn> result = new List<KanbanColumn>();
            foreach (KanbanColumn column in chat.KanbanColumns)
            {
                result.Add(column);
            }
            return result;
        }

        
        public static string AddColumn(Chat chat, string name)
        {
            
            foreach (KanbanColumn column in chat.KanbanColumns)
            {
                if (column.Name == name)
                {
                    return "Колонка с таким названием уже существует";
                }
            }

            KanbanColumn newColumn = new KanbanColumn();
            newColumn.Name = name;
            newColumn.Color = "#9E9E9E";
            chat.KanbanColumns.Add(newColumn);
            return null;
        }

        
        public static void DeleteColumn(Chat chat, KanbanColumn column)
        {
            
            List<TaskItem> tasksToRemove = new List<TaskItem>();
            foreach (TaskItem task in chat.Tasks)
            {
                if (task.Status == column.Name)
                {
                    tasksToRemove.Add(task);
                }
            }
            foreach (TaskItem task in tasksToRemove)
            {
                chat.Tasks.Remove(task);
            }

            chat.KanbanColumns.Remove(column);
        }

        
        public static void MoveColumn(Chat chat, KanbanColumn column, int direction)
        {
            
            int oldIndex = chat.KanbanColumns.IndexOf(column);
            int newIndex = oldIndex + direction;

            if (oldIndex < 0 || newIndex < 0 || newIndex >= chat.KanbanColumns.Count)
            {
                return;
            }

            chat.KanbanColumns.RemoveAt(oldIndex);
            chat.KanbanColumns.Insert(newIndex, column);
        }

        
        public static string UpdateColumn(Chat chat, KanbanColumn column, string newName, string newColor)
        {
            
            if (newName != column.Name)
            {
                foreach (KanbanColumn other in chat.KanbanColumns)
                {
                    if (other != column && other.Name == newName)
                    {
                        return "Колонка с таким названием уже существует";
                    }
                }

                
                foreach (TaskItem task in chat.Tasks)
                {
                    if (task.Status == column.Name)
                    {
                        task.Status = newName;
                    }
                }
            }

            column.Name = newName;
            column.Color = newColor;
            return null;
        }
    }
}
