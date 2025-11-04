using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace DashboardBlazor.Services
{
    public record RabbitQueueInfo(string name, int messages_ready, int messages_unacknowledged, int consumers);

    public class RabbitMqService
    {
        private readonly HttpClient _http;
        private readonly string _baseApi;
        private readonly string _username;
        private readonly string _password;

        public RabbitMqService(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _baseApi = configuration.GetValue<string>("RabbitMQ:ManagementUrl")?.TrimEnd('/') ?? "http://localhost:15672/api";
            _username = configuration.GetValue<string>("RabbitMQ:Username") ?? "user";
            _password = configuration.GetValue<string>("RabbitMQ:Password") ?? "password";

            var auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_username}:{_password}"));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        }

        // returns list of queues (name and message counts)
        public async Task<IEnumerable<RabbitQueueInfo>> GetQueuesAsync()
        {
            // RabbitMQ Management API: /api/queues
            try
            {
                var url = $"{_baseApi}/queues";
                var arr = await _http.GetFromJsonAsync<JsonElement[]>(url);
                if (arr == null) return Enumerable.Empty<RabbitQueueInfo>();

                var list = arr.Select(q =>
                {
                    var name = q.GetProperty("name").GetString() ?? string.Empty;
                    var messagesReady = q.TryGetProperty("messages_ready", out var mr) ? mr.GetInt32() : 0;
                    var messagesUnacked = q.TryGetProperty("messages_unacknowledged", out var mu) ? mu.GetInt32() : 0;
                    var consumers = q.TryGetProperty("consumers", out var c) ? c.GetInt32() : 0;
                    return new RabbitQueueInfo(name, messagesReady, messagesUnacked, consumers);
                }).ToList();

                return list;
            }
            catch
            {
                return Enumerable.Empty<RabbitQueueInfo>();
            }
        }
    }
}
