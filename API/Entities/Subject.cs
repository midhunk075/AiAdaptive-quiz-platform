using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Subject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public required string SubjectName { get; set; }    
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string MentorId { get; set; } = string.Empty;
        public AppUser Mentor { get; set; } = null!;
        public Syllabus? Syllabus { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public string Status { get; set; } = "draft";
        public ICollection<Topic> Topics { get; set; } = new List<Topic>();
        public bool HasSyllabus { get; set; } = false;
    }
}