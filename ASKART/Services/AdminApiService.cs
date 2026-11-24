using Askart.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Askart.Services
{
    // ───────────────────────── Ошибка API ─────────────────────────

    public class ApiException : Exception
    {
        public System.Net.HttpStatusCode? StatusCode { get; }
        public ApiException(string message, System.Net.HttpStatusCode? statusCode = null) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    // ───────────────────────── Сервис ─────────────────────────

    // HTTP-клиент административных операций.
    public sealed class AdminApiService
    {
        private readonly HttpClient client;
        private readonly SessionService session;
        private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public AdminApiService(HttpClient client, SessionService session)
        {
            this.client = client;
            this.session = session;
        }

        // ── Пользователи ──

        public async Task<PagedResult<AdminUserDto>> GetUsersAsync(string search, int skip, int take, CancellationToken cancellationToken = default)
        {
            string url = "admin/users?skip=" + skip + "&take=" + take
                + "&search=" + Uri.EscapeDataString(search ?? "");
            return await GetAsync<PagedResult<AdminUserDto>>(url, cancellationToken);
        }

        // ── Чаты и сообщения ──

        public async Task<PagedResult<AdminChatDto>> GetChatsAsync(
            string search, string? typeChat, string? typeGroup, int skip, int take, CancellationToken cancellationToken = default)
        {
            string url = "admin/chats?skip=" + skip + "&take=" + take
                + "&search=" + Uri.EscapeDataString(search ?? "");

            if (typeChat != null)
            {
                url += "&typeChat=" + Uri.EscapeDataString(typeChat);
            }
            if (typeGroup != null)
            {
                url += "&typeGroup=" + Uri.EscapeDataString(typeGroup);
            }

            return await GetAsync<PagedResult<AdminChatDto>>(url, cancellationToken);
        }

        public async Task<PagedResult<AdminMessageDto>> GetChatMessagesAsync(int chatId, CancellationToken cancellationToken = default)
        {
            return await GetAsync<PagedResult<AdminMessageDto>>("admin/chats/" + chatId + "/messages", cancellationToken);
        }

        public async Task DeleteMessageAsync(int messageId, CancellationToken cancellationToken = default)
        {
            await DeleteAsync("admin/messages/" + messageId, cancellationToken);
        }

        public async Task DeleteChatAsync(int chatId, CancellationToken cancellationToken = default)
        {
            await DeleteAsync("admin/chats/" + chatId, cancellationToken);
        }

        // ── Статистика ──

        public async Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<AdminStatsDto>("admin/stats", cancellationToken);
        }

        // ───────────────────────── Внутреннее ─────────────────────────

        private async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await SendAsync(HttpMethod.Get, url, null, true, cancellationToken);
            using (response)
            {
                await EnsureSuccessAsync(response, cancellationToken);
                T? data = await ReadJsonAsync<T>(response, cancellationToken);
                if (data == null)
                {
                    throw new ApiException("Сервер вернул пустой ответ.");
                }
                return data;
            }
        }

        private async Task DeleteAsync(string url, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await SendAsync(HttpMethod.Delete, url, null, true, cancellationToken);
            using (response)
            {
                await EnsureSuccessAsync(response, cancellationToken);
            }
        }

        private async Task<HttpResponseMessage> SendAsync(
            HttpMethod method, string url, HttpContent? content, bool withAdminHeader, CancellationToken cancellationToken)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(method, url))
            {
                request.Content = content;

                if (withAdminHeader)
                {
                    int adminId = session.CurrentUser?.Id ?? 0;
                    request.Headers.Add("X-Admin-Id", adminId.ToString());
                }

                try
                {
                    return await client.SendAsync(request, cancellationToken);
                }
                catch (HttpRequestException)
                {
                    throw new ApiException("Не удалось подключиться к серверу (" + ApiConfiguration.BaseAddress + "). Проверьте, что API запущен.");
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                catch (TaskCanceledException)
                {
                    throw new ApiException("Сервер не отвечает: превышено время ожидания.");
                }
            }
        }

        private async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<T>(jsonOptions, cancellationToken);
            }
            catch (Exception ex) when (ex is JsonException || ex is NotSupportedException)
            {
                throw new ApiException("Сервер вернул ответ в неожиданном формате.");
            }
        }

        private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string message = "Ошибка сервера (" + (int)response.StatusCode + ").";

            try
            {
                string body = await response.Content.ReadAsStringAsync(cancellationToken);
                using (JsonDocument document = JsonDocument.Parse(body))
                {
                    if (document.RootElement.ValueKind == JsonValueKind.Object &&
                        document.RootElement.TryGetProperty("message", out JsonElement messageElement) &&
                        messageElement.ValueKind == JsonValueKind.String)
                    {
                        message = messageElement.GetString() ?? message;
                    }
                }
            }
            catch (JsonException)
            {
                // тело не JSON — оставляем сообщение по умолчанию
            }

            throw new ApiException(message, response.StatusCode);
        }
    }
}