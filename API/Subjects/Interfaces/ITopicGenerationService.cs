namespace API.Subjects.Interfaces
{
    public interface ITopicGenerationService
    {
        Task<List<string>> ExtractTopicsAsync(string syllabusText);
    }
}
