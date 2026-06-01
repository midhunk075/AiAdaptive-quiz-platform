using System.ComponentModel.DataAnnotations;

namespace API.Subjects.DTO
{
    public class SubjectResponseDTO
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public required string SubjectName { get; set; }
        public required string Description { get; set; } 
        public string Status { get; set; } = "draft";
        public int QuestionCount { get; set; }
        public bool HasSyllabus { get; set; }
    }
}
