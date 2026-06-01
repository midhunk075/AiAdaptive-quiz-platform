using API.Subjects.DTO;

namespace API.Subjects.Interfaces
{
    public interface ISubjectService
    {
        Task<IEnumerable<SubjectResponseDTO>> GetMentorSubjectsAsync(string mentorId);
        Task<SubjectResponseDTO> GetSubjectByIdAsync(string subjectId, string mentorId);
        Task<SubjectResponseDTO> CreateSubjectAsync(SubjectCreateDTO subjectCreateDTO, string mentorId);
        Task DeleteSubjectAsync(string subjectId, string mentorId);
        Task<IReadOnlyList<TopicResponseDTO>> ProcessSyllabusUploadAsync(string subjectId, string mentorId, IFormFile file);
        Task<IReadOnlyList<TopicResponseDTO>> GetSubjectTopicsAsync(string subjectId, string mentorId);
        Task<IReadOnlyList<TopicResponseDTO>> UpdateTopicSelectionAsync(string subjectId, string mentorId, UpdateTopicSelectionDTO request);
        Task<QuizGenerationResponseDTO> ProcessAdaptiveQuizGenerationAsync(string subjectId, string mentorId, AdaptiveQuizRequestDTO request);
        Task<IReadOnlyList<SubjectQuestionDTO>> GetSubjectQuestionsAsync(string subjectId, string mentorId);
        Task<SubjectQuestionDTO> SetQuestionApprovalAsync(string subjectId, string questionId, string mentorId, bool isApproved);
        Task DiscardQuestionAsync(string subjectId, string questionId, string mentorId);
    }
}
