using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Question
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string QuestionText { get; set; } = null!;
        [Range(1, 3)]
        public int DifficultyLevel { get; set; }
        public string SubjectId { get; set; } = string.Empty;
        public Subject Subject { get; set; } = null!;
        public ICollection<Option> Options { get; set; } = new List<Option>();
        public bool IsApproved { get; set; } = false;
        public string TopicId { get; set; } = string.Empty;
        public Topic Topic { get; set; } = null!;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }
}