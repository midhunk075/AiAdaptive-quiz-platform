namespace API.Subjects.DTO
{
    public class UpdateTopicSelectionDTO
    {
        public List<TopicSelectionItemDTO> Topics { get; set; } = new();
    }

    public class TopicSelectionItemDTO
    {
        public string TopicId { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
