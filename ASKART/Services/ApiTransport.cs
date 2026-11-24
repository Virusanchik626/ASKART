using Askart.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
namespace Askart.Services;

public sealed class ApiTransport(HttpClient client)
{
    private readonly string baseUrl = ApiConfiguration.BaseAddress.AbsoluteUri;
    private CancellationTokenSource sessionRequests = new();
    public void ResetSession()
    {
        var previous = sessionRequests;
        sessionRequests = new();
        previous.Cancel();
        previous.Dispose();
        LastError = "";
    }
    public string LastError { get; set; } = "";
    public event Action<string>? ErrorOccurred;
    public static JsonSerializerOptions JsonOptions { get; } = new() { PropertyNameCaseInsensitive = true };
        public async Task<string> SendAsync(HttpMethod method, string url, string json, CancellationToken cancellationToken = default)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(sessionRequests.Token, cancellationToken);
            cancellationToken = linked.Token;
            LastError = "";
            try
            {
                using HttpRequestMessage request = new HttpRequestMessage(method, baseUrl + url);
                if (json != null)
                {
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
                string body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return body;
                }

                string text = GetErrorText(body);
                if (text == "")
                {
                    text = "Сервер ответил кодом " + (int)response.StatusCode + " " + response.ReasonPhrase;
                }
                LastError = text;
                return null;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException)
            {
                LastError = "Нет связи с сервером (запущен ли докер?): " + ex.Message;
                return null;
            }
        }

        private static string GetErrorText(string body)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(body);
                JsonElement root = doc.RootElement;

                JsonElement messageElement;
                if (root.TryGetProperty("message", out messageElement))
                {
                    return messageElement.GetString();
                }

                JsonElement errorsElement;
                if (root.TryGetProperty("errors", out errorsElement))
                {
                    foreach (JsonProperty prop in errorsElement.EnumerateObject())
                    {
                        foreach (JsonElement item in prop.Value.EnumerateArray())
                        {
                            return item.GetString();
                        }
                    }
                }
            }
            catch
            {
				
            }
            return "";
        }

        public void NotifyError()
        {
            ErrorOccurred?.Invoke(LastError);
        }


}
