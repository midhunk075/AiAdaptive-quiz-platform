namespace API.Entities
{
    public class Topic
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Name { get; set; }
        public string SubjectId { get; set; } = string.Empty;
        public Subject Subject { get; set; } = null!;
        public bool IsSelected { get; set; } = false;
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
