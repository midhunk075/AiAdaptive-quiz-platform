namespace API.Subjects.DTO;

public class QuizGenerationResponseDTO
{
    public List<GeneratedQuestionDTO> Questions { get; set; } = new();
}

public class GeneratedQuestionDTO
{
    public string QuestionText { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}

public class GenerateQuizRequestDTO
{
    public int DifficultyLevel { get; set; } 
    public int QuestionCount { get; set; }   
}
