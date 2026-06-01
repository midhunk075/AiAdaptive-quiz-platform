using API.Subjects.DTO;

namespace API.Subjects.Interfaces;

public interface IQuestionGeneratorService
{
    Task<QuizGenerationResponseDTO> GenerateQuestionsAsync(string topicName, int difficultyLevel, int count);
}