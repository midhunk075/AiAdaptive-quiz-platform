using API.Entities;

namespace API.Subjects.Interfaces
{
    public interface ISubjectRepository
    {
        Task<Subject?> GetSubjectByIdAsync(string id);
        Task<IEnumerable<Subject>> GetSubjectsByMentorIdAsync(string mentorId);
        void DeleteSubject(Subject subject);
        Task<bool> AddTopicsToSubjects(string subjectId, IEnumerable<Topic> topics);
        Task<List<Topic>> GetTopicsBySubjectIdAsync(string subjectId);
        Task SaveTopicSelectionAsync(IEnumerable<Topic> topics);
        Task<Topic?> GetTopicWithSubjectCheckAsync(string topicId);
        Task<List<Topic>> GetTopicsWithSubjectByIdsAsync(IEnumerable<string> topicIds);
        Task<IEnumerable<string>> GetTopicNamesByIdsAsync(IEnumerable<string> topicIds);
        Task SaveQuizQuestionsBatchAsync(IEnumerable<Question> questions);
        Task<List<Question>> GetQuestionsBySubjectIdAsync(string subjectId);
        Task<Question?> GetQuestionWithRelationsAsync(string questionId);
        void DeleteQuestion(Question question);
        Task AddAsync(Subject subject);
        Task SaveChangesAsync();
    }
}
