using API.Exceptions;
using API.Subjects.DTO;
using API.Subjects.Interfaces;
using Google.GenAI;
using Google.GenAI.Types;
using System.Text.Json;

namespace API.Subjects.Services;

public class QuestionGeneratorService : IQuestionGeneratorService
{
    private readonly Client _client;
    private const string ModelName = "gemini-2.5-flash";

    public QuestionGeneratorService(IConfiguration config)
    {
        var apiKey = config["GeminiSettings:ApiKey"] ?? throw new ArgumentNullException("Gemini API key is unconfigured.");
        _client = new Client(apiKey: apiKey);
    }

    public async Task<QuizGenerationResponseDTO> GenerateQuestionsAsync(string topicName, int difficultyLevel, int count)
    {
        string difficultyDescription = difficultyLevel switch
        {
            1 => "Easy (Foundational syntax, core definitions, basic concepts)",
            2 => "Medium (Code analysis, execution scenarios, implementation choices)",
            3 => "Hard (Optimization, deep troubleshooting, multi-threading edge-cases, system debugging)",
            _ => "Medium"
        };

        var systemInstruction = "You are an elite technical interview assessor and adaptive learning system. " +
                                "Your task is to generate high-quality multiple-choice questions based strictly on the target topic and required difficulty level. " +
                                "Rules:\n" +
                                "1. Each question must have exactly 4 unique options.\n" +
                                "2. The 'CorrectAnswer' string must exactly match one of the items inside the options array.\n" +
                                "3. The 'Explanation' must provide sound technical reasoning for why that choice is correct.\n" +
                                "4. Return valid JSON only. Do not include markdown fences or extra prose.";

        var targetPrompt = $"Generate exactly {count} technical multiple-choice questions for the following context:\n" +
                           $"Topic: {topicName}\n" +
                           $"Target Difficulty: {difficultyDescription}\n\n" +
                           $"Return the output as a clean, single JSON object matching this schema shape:\n" +
                           $"{{\"questions\": [{{\"questionText\": \"string\", \"options\": [\"string\", \"string\", \"string\", \"string\"], \"correctAnswer\": \"string\", \"explanation\": \"string\"}}]}}";

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
                contents: targetPrompt,
                config: config
            )
        );

        if (string.IsNullOrWhiteSpace(response.Text))
            throw new ServiceUnavailableException("Gemini returned an empty response. Please try again in a moment.");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        var quizData = JsonSerializer.Deserialize<QuizGenerationResponseDTO>(response.Text, options);

        return quizData ?? throw new ValidationException("Failed to map the AI response into generated questions.");
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
