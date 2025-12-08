using System.Text.Json.Serialization;

namespace InformacinesSistemos.Services
{
    public class GroqAIService : IAIService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GroqAIService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Groq:ApiKey"]
                      ?? throw new InvalidOperationException("Groq API key missing");
        }

        public async Task<string> GetAnswerAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return "Klausimas negali būti tuščias.";

            var requestBody = new
            {
                model = "openai/gpt-oss-120b",
                messages = new[]
                {
                    new { role = "user", content = question }
                }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.groq.com/openai/v1/chat/completions"
            );
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = JsonContent.Create(requestBody);

            try
            {
                var response = await _http.SendAsync(request);

                // Read the raw content for debugging
                var responseText = await response.Content.ReadAsStringAsync();
               // Console.WriteLine(responseText); // <- inspect this

                if (!response.IsSuccessStatusCode)
                {
                    return $"Klaida: {response.StatusCode} - {responseText}";
                }

                var json = System.Text.Json.JsonSerializer.Deserialize<GroqResponse>(responseText);

                return json?.Choices?.FirstOrDefault()?.Message?.Content
                       ?? "Nepavyko gauti atsakymo.";
            }
            catch (HttpRequestException ex)
            {
                return $"Tinklo klaida: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Nežinoma klaida: {ex.Message}";
            }
        }

        private class GroqResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; } = new();

            public class Choice
            {
                [JsonPropertyName("message")]
                public Message Message { get; set; } = new();
            }

            public class Message
            {
                [JsonPropertyName("role")]
                public string Role { get; set; } = "";

                [JsonPropertyName("content")]
                public string Content { get; set; } = "";
            }
        }
    }
}
