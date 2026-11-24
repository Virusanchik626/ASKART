using Askart.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Askart.Services;

public sealed class AuthenticationService(HttpClient client, SessionService session)
{
    public async Task<User> LoginAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("Users/login", new { login, password }, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new AuthenticationException("Не удалось подключиться к API.", ex);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AuthenticationException("API не отвечает: превышено время ожидания.", ex);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new AuthenticationException("Неверная почта или пароль");
            if (!response.IsSuccessStatusCode)
                throw new AuthenticationException($"Ошибка сервера ({(int)response.StatusCode}).");

            ApiLoginUser data;
            try
            {
                data = await response.Content.ReadFromJsonAsync<ApiLoginUser>(cancellationToken)
                    ?? throw new JsonException("Пустой ответ");
            }
            catch (JsonException ex)
            {
                throw new AuthenticationException("API вернул некорректный ответ.", ex);
            }

            var user = new User
            {
                Id = data.Id, Surname = data.Surname, Name = data.Name,
                Lastname = data.Lastname, Login = data.Login, Email = data.Email,
                Number = data.Number, IsAdmin = data.IsAdmin
            };
            session.SignIn(user);
            return user;
        }
    }
}

public sealed class AuthenticationException(string message, Exception? inner = null) : Exception(message, inner);
