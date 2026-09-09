using Askart.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Askart.MockData
{
    public static class DataInitializer
    {
        public static ObservableCollection<User> Users { get; set; }
        public static ObservableCollection<Chat> Chats { get; set; }

        static DataInitializer()
        {
            InitializeUsers();
            InitializeChats();
        }

        private static void InitializeUsers()
        {
            Users = new ObservableCollection<User>
            {
                new User
                {
                    Id = 1, Surname = "Иванов", Name = "Иван", Lastname = "Иванович",
                    Login = "test@test.com", Password = "12345678", // 8 символов!
                    Email = "test@test.com", Number = "+79991234567",
                    RegistrationDate = DateTime.Now
                },
                new User
                {
                    Id = 2, Surname = "Петров", Name = "Петр", Lastname = "Петрович",
                    Login = "petrov@test.com", Password = "12345678",
                    Email = "petrov@test.com", Number = "+79991112233",
                    RegistrationDate = DateTime.Now
                },
                new User
                {
                    Id = 3, Surname = "Сидоров", Name = "Сидор", Lastname = "Сидорович",
                    Login = "sidorov@test.com", Password = "12345678",
                    Email = "sidorov@test.com", Number = "+79994445566",
                    RegistrationDate = DateTime.Now
                }
            };
        }

        private static void InitializeChats()
        {
            Chats = new ObservableCollection<Chat>
            {
                // Групповой чат (3 участника) — с правильными ролями
                new Chat
                {
                    Id = 1, Name = "Общий чат",
                    TypeChat = "Группа", TypeGroup = "Личная",
                    CreatedAt = DateTime.Now.AddDays(-30),
                    OwnerId = 1,
                    Members = new List<ChatMember>
                    {
                        new ChatMember { ChatId = 1, UserId = 1, Role = "Владелец" },
                        new ChatMember { ChatId = 1, UserId = 2, Role = "Администратор" },
                        new ChatMember { ChatId = 1, UserId = 3, Role = "Участник" }
                    },
                    Messages = new ObservableCollection<Message>
                    {
                        new Message
                        {
                            Id = 1, Text = "Всем привет!", ChatId = 1, SenderId = 1,
                            SendTime = DateTime.Now.AddHours(-5), SenderName = "Иванов Иван"
                        },
                        new Message
                        {
                            Id = 2, Text = "Привет! Как дела?", ChatId = 1, SenderId = 2,
                            SendTime = DateTime.Now.AddHours(-4), SenderName = "Петров Петр",
                            ReplyId = 1
                        }
                    }
                },
                // Рабочий чат с задачами
                new Chat
                {
                    Id = 2, Name = "Рабочая группа",
                    TypeChat = "Группа", TypeGroup = "Рабочая",
                    CreatedAt = DateTime.Now.AddDays(-10),
                    OwnerId = 1,
                    KanbanColumns = new List<KanbanColumn>
                    {
                        new KanbanColumn { Name = "Новая", Color = "#9E9E9E" },
                        new KanbanColumn { Name = "В работе", Color = "#5DD4FF" },
                        new KanbanColumn { Name = "Выполнена", Color = "#4CAF50" }
                    },
                    Members = new List<ChatMember>
                    {
                        new ChatMember { ChatId = 2, UserId = 1, Role = "Владелец" },
                        new ChatMember { ChatId = 2, UserId = 2, Role = "Участник" }
                    },
                    Tasks = new ObservableCollection<TaskItem>
                    {
                        new TaskItem
                        {
                            Id = 1, ChatId = 2, Name = "Разработать дизайн",
                            Description = "Создать макеты", Status = "В работе",
                            Deadline = DateTime.Now.AddDays(5), Priority = "Высокий",
                            CreatorId = 1, AssigneeUserId = 2, AssigneeName = "Петров Петр",
                            CreatedAt = DateTime.Now.AddDays(-5)
                        },
                        new TaskItem
                        {
                            Id = 2, ChatId = 2, Name = "Написать документацию",
                            Description = "Описать API", Status = "Новая",
                            Deadline = DateTime.Now.AddDays(10), Priority = "Средний",
                            CreatorId = 1, AssigneeUserId = 2, AssigneeName = "Петров Петр",
                            CreatedAt = DateTime.Now.AddDays(-2)
                        },
                        new TaskItem
                        {
                            Id = 3, ChatId = 2, Name = "Тестирование",
                            Description = "Протестировать", Status = "Выполнена",
                            Deadline = DateTime.Now.AddDays(-2), Priority = "Низкий",
                            CreatorId = 1, AssigneeUserId = 2, AssigneeName = "Петров Петр",
                            CreatedAt = DateTime.Now.AddDays(-10),
                            CompletedAt = DateTime.Now.AddDays(-3)
                        }
                    }
                },
                // Личный чат (2 участника)
                new Chat
                {
                    Id = 3, Name = "Личка с Петровым",
                    TypeChat = "Личный", TypeGroup = null,
                    CreatedAt = DateTime.Now.AddDays(-15),
                    OwnerId = 1,
                    Members = new List<ChatMember>
                    {
                        new ChatMember { ChatId = 3, UserId = 1, Role = "Владелец" },
                        new ChatMember { ChatId = 3, UserId = 2, Role = "Участник" }
                    },
                    Messages = new ObservableCollection<Message>
                    {
                        new Message
                        {
                            Id = 1, Text = "Привет! Как дела?", ChatId = 3, SenderId = 2,
                            SendTime = DateTime.Now.AddHours(-2), SenderName = "Петров Петр"
                        },
                        new Message
                        {
                            Id = 2, Text = "Привет! Всё отлично, работаю над проектом",
                            ChatId = 3, SenderId = 1,
                            SendTime = DateTime.Now.AddHours(-1), SenderName = "Иванов Иван"
                        }
                    }
                }
            };
        }

        public static User Authenticate(string login, string password)
        {
            return Users.FirstOrDefault(u => u.Login == login && u.Password == password);
        }

        public static bool RegisterUser(string name, string surname, string email, string password, string phone)
        {
            if (!User.IsValidEmail(email))
                return false;
            if (!User.IsValidPassword(password))
                return false;
            if (Users.Any(u => u.Login == email || u.Email == email))
                return false;

            var newUser = new User
            {
                Id = Users.Count > 0 ? Users.Max(u => u.Id) + 1 : 1,
                Name = name,
                Surname = surname,
                Lastname = "",
                Login = email,
                Password = password,
                Email = email,
                Number = phone ?? "",
                RegistrationDate = DateTime.Now
            };

            Users.Add(newUser);
            return true;
        }
    }
}