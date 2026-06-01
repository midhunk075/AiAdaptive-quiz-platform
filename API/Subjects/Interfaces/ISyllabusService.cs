namespace API.Subjects.Interfaces
{
    public interface ISyllabusService
    {
        Task<string> ExtractTextAsync(IFormFile file);
        Task<List<string>> ExtractTopicsWithAIAsync(string rawText);
    }
}
