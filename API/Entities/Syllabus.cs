using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Syllabus
    {
        [Required]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? ContentText { get; set; }
        public string? FilePath { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string SubjectId { get; set; } = string.Empty;
        public Subject Subject { get; set; } = null!;
    }
}