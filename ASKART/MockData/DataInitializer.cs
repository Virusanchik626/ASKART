using Askart.Models;
using System.Collections.ObjectModel;

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
                    Id = 1,
                    Surname = "Иванов",
                    Name = "Иван",
                    Lastname = "Иванович",
                    Login = "test@test.com",
                    Password = "123456",
                    Email = "test@test.com",
                    Number = "+7 (999) 123-45-67",
                    RegistrationDate = DateTime.Now
                },
                new User
                {
                    Id = 2,
                    Surname = "Петров",
                    Name = "Петр",
                    Lastname = "Петрович",
                    Login = "petrov@test.com",
                    Password = "123456",
                    Email = "petrov@test.com",
                    Number = "+7 (999) 111-22-33",
                    RegistrationDate = DateTime.Now
                },
                new User
                {
                    Id = 3,
                    Surname = "Сидоров",
                    Name = "Сидор",
                    Lastname = "Сидорович",
                    Login = "sidorov@test.com",
                    Password = "123456",
                    Email = "sidorov@test.com",
                    Number = "+7 (999) 444-55-66",
                    RegistrationDate = DateTime.Now
                }
            };
        }

        private static void InitializeChats()
        {
            Chats = new ObservableCollection<Chat>
            {
                new Chat
                {
                    Id = 1,
                    Name = "Общий чат",
                    TypeChat = "Группа",
                    TypeGroup = "Личная",
                    OwnerId = 1,
                    Members = new List<ChatMember>
                    {
                        new ChatMember { ChatId = 1, UserId = 1, Role = "Управляющий" },
                        new ChatMember { ChatId = 1, UserId = 2, Role = "Работник" },
                        new ChatMember { ChatId = 1, UserId = 3, Role = "Работник" }
                    }
                },
                new Chat
                {
                    Id = 2,
                    Name = "Рабочая группа",
                    TypeChat = "Группа",
                    TypeGroup = "Рабочая",
                    OwnerId = 1,
                    Members = new List<ChatMember>
                    {
                        new ChatMember { ChatId = 2, UserId = 1, Role = "Управляющий" },
                        new ChatMember { ChatId = 2, UserId = 2, Role = "Работник" }
                    },
                    Tasks = new ObservableCollection<TaskItem>
                    {
                        new TaskItem
                        {
                            Id = 1,
                            ChatId = 2,
                            Name = "Разработать дизайн",
                            Description = "Создать макеты интерфейса",
                            Status = "В работе",
                            Deadline = DateTime.Now.AddDays(5),
                            Priority = "Высокий",
                            CreatorId = 1,
                            AssigneeUserId = 2,
                            AssigneeName = "Петров Петр"
                        },
                        new TaskItem
                        {
                            Id = 2,
                            ChatId = 2,
                            Name = "Написать документацию",
                            Description = "Описать API endpoints",
                            Status = "Новая",
                            Deadline = DateTime.Now.AddDays(10),
                            Priority = "Средний",
                            CreatorId = 1,
                            AssigneeUserId = 2,
                            AssigneeName = "Петров Петр"
                        },
                        new TaskItem
                        {
                            Id = 3,
                            ChatId = 2,
                            Name = "Тестирование",
                            Description = "Протестировать функционал",
                            Status = "Выполнена",
                            Deadline = DateTime.Now.AddDays(-2),
                            Priority = "Низкий",
                            CreatorId = 1,
                            AssigneeUserId = 2,
                            AssigneeName = "Петров Петр"
                        }
                    }
                },
                new Chat
                {
                    Id = 3,
                    Name = "Личка с Петровым",
                    TypeChat = "Личный",
                    TypeGroup = null,
                    OwnerId = 1,
                    Members = new List<ChatMember>
                    {
                        new ChatMember { ChatId = 3, UserId = 1, Role = "Управляющий" },
                        new ChatMember { ChatId = 3, UserId = 2, Role = "Работник" }
                    },
                    Messages = new ObservableCollection<Message>
                    {
                        new Message
                        {
                            Id = 1,
                            Text = "Привет! Как дела?",
                            ChatId = 3,
                            SenderId = 2,
                            SendTime = DateTime.Now.AddHours(-2),
                            SenderName = "Петров Петр"
                        },
                        new Message
                        {
                            Id = 2,
                            Text = "Привет! Всё отлично, работаю над проектом",
                            ChatId = 3,
                            SenderId = 1,
                            SendTime = DateTime.Now.AddHours(-1),
                            SenderName = "Иванов Иван"
                        }
                    }
                }
            };
        }

        public static User Authenticate(string login, string password)
        {
            return Users.FirstOrDefault(u => u.Login == login && u.Password == password);
        }

        public static bool RegisterUser(string name, string surname, string email, string password)
        {
            if (Users.Any(u => u.Login == email))
                return false;

            var newUser = new User
            {
                Id = Users.Max(u => u.Id) + 1,
                Name = name,
                Surname = surname,
                Lastname = "",
                Login = email,
                Password = password,
                Email = email,
                Number = "",
                RegistrationDate = DateTime.Now
            };

            Users.Add(newUser);
            return true;
        }
    }
}