using Askart.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
namespace Askart.Services;

public sealed class UserDirectoryService
{
    private readonly ApiTransport api;
    internal readonly List<User> allUsers = new();
    public UserDirectoryService(ApiTransport api) { this.api = api; }
    public void ClearCache() => allUsers.Clear();
        public async Task<string> Register(string name, string surname, string email, string password, string phone, CancellationToken cancellationToken = default)
        {
            if (phone == "")
            {
                return "Укажите номер телефона, например +79991234567";
            }

            // Логин в базе до 30 символов, а мы кладём туда почту
            if (email.Length > 30)
            {
                return "Почта слишком длинная: максимум 30 символов";
            }

            // Убираем пробелы, скобки и тире, сервер ждёт чистый +79991234567
            string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            string json = JsonSerializer.Serialize(new
            {
                surname = surname,
                name = name,
                lastname = "",
                login = email,
                password = password,
                number = cleanPhone,
                email = email
            });

            string answer = await api.SendAsync(HttpMethod.Post, "Users/register", json, cancellationToken);
            if (answer == null)
            {
                return api.LastError;
            }

            allUsers.Clear();
            return null;
        }

        

        public async Task<User> GetUserById(int userId, CancellationToken cancellationToken = default)
        {
            User found = FindUserInCache(userId);
            if (found != null)
            {
                return found;
            }

            await LoadUsers(cancellationToken);
            return FindUserInCache(userId);
        }

        private User FindUserInCache(int userId)
        {
            foreach (User user in allUsers)
            {
                if (user.Id == userId)
                {
                    return user;
                }
            }
            return null;
        }

        internal async Task LoadUsers(CancellationToken cancellationToken = default)
        {
            string json = await api.SendAsync(HttpMethod.Get, "Users", null, cancellationToken);
            if (json == null)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            List<ApiUser> list = JsonSerializer.Deserialize<List<ApiUser>>(json, ApiTransport.JsonOptions);
            allUsers.Clear();
            foreach (ApiUser a in list)
            {
                User user = new User();
                user.Id = a.IdUser;
                user.Surname = a.Surname;
                user.Name = a.Name;
                user.Lastname = a.Lastname;
                user.Login = a.Login;
                user.Password = a.Password;
                user.Email = a.Email;
                user.Number = a.Number;
                user.RegistrationDate = a.DataReg;
                allUsers.Add(user);
            }
        }

}
