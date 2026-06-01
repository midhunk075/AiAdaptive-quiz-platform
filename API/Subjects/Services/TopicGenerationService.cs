using API.Exceptions;
using API.Subjects.Interfaces;
using Google.GenAI;
using Google.GenAI.Types;
using System.Text.Json;

namespace API.Subjects.Services
{
    public class TopicGenerationService : ITopicGenerationService
    {
        private readonly Client _client;
        private const string ModelName = "gemini-2.5-flash";

        public TopicGenerationService(IConfiguration config)
        {
            var apiKey = config["GeminiSettings:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Gemini API key is missing. Configure GeminiSettings:ApiKey.");
            }

            _client = new Client(apiKey: apiKey);
        }

        public async Task<List<string>> ExtractTopicsAsync(string syllabusText)
        {
            var systemInstruction = "You are a Technical Curriculum Architect. " +
                                    "Extract core technical modules from the text. " +
                                    "Return ONLY a JSON list of strings.";
            var config = new GenerateContentConfig
            {
                SystemInstruction = new Content
                {
                    Parts = new List<Part> { new Part { Text = systemInstruction } }
                },
                ResponseMimeType = "application/json"
            };

            var response = await ExecuteWithRetryAsync(
                () => _client.Models.GenerateContentAsync(
                    model: ModelName,
                    contents: syllabusText,
                    config: config
                )
            );

            if (string.IsNullOrWhiteSpace(response.Text))
            {
                return new List<string>();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            return JsonSerializer.Deserialize<List<string>>(response.Text, options) ?? new List<string>();
        }

        private static async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
        {
            var delays = new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4)
            };

            for (var attempt = 0; attempt <= delays.Length; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex) when (IsTransientGeminiError(ex) && attempt < delays.Length)
                {
                    await Task.Delay(delays[attempt]);
                }
                catch (Exception ex) when (IsTransientGeminiError(ex))
                {
                    throw new ServiceUnavailableException("Gemini is currently experiencing high demand. Please try again in a moment.");
                }
            }

            throw new ServiceUnavailableException("Gemini is currently unavailable. Please try again in a moment.");
        }

        private static bool IsTransientGeminiError(Exception ex)
        {
            var message = ex.Message.ToLowerInvariant();
            var typeName = ex.GetType().Name.ToLowerInvariant();

            return typeName.Contains("servererror") ||
                   message.Contains("high demand") ||
                   message.Contains("try again later") ||
                   message.Contains("temporarily unavailable") ||
                   message.Contains("resource exhausted") ||
                   message.Contains("429") ||
                   message.Contains("503");
        }
    }
}
