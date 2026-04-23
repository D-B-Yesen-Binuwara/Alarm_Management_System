using INMS.Application.Interfaces;
using System.Net.Http.Json;

namespace INMS.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private const string OllamaUrl = "http://localhost:11434/api/generate";
        private const string SystemPrompt = @"You are a Network Operations Assistant for an Integrated Network Management System.
You understand SLBN, CEAN, MSAN layers.
You answer only network-related questions.
Be concise and technical.";

        public ChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(120);
        }

        public async Task<string> GetChatResponseAsync(string userMessage)
        {
            var prompt = $"{SystemPrompt}\n\nUser: {userMessage}\nAssistant:";

            var requestBody = new
            {
                model = "llama3",
                prompt = prompt,
                stream = false
            };

            var response = await _httpClient.PostAsJsonAsync(OllamaUrl, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get response from Ollama: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }

            var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaResponse>();

            return ollamaResponse?.Response?.Trim() ?? "Sorry, I couldn't generate a response.";
        }

        private class OllamaResponse
        {
            public string Response { get; set; } = string.Empty;
        }
    }
}