using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace API.Entities
{
    public class Option
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string OptionText { get; set; }
        public string QuestionId { get; set; } = string.Empty;
        public Question Question { get; set; } = null!;
    }
}