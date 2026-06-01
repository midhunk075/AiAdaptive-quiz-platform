namespace API.Subjects.DTO;

public class SubjectQuestionDTO
{
    public string Id { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    public bool IsApproved { get; set; }
    public string TopicId { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}
