using System.Security.Claims;
using API.Exceptions;
using API.Subjects.DTO;
using API.Subjects.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Subjects.Controllers
{
    [Authorize(Roles = "mentor,Mentor")]
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectsController(ISubjectService subjectService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubjectResponseDTO>>> GetMySubjects()
        {
            var mentorId = GetMentorIdOrThrow();
            var subjects = await subjectService.GetMentorSubjectsAsync(mentorId);
            return Ok(subjects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubjectResponseDTO>> GetSubject(string id)
        {
            var mentorId = GetMentorIdOrThrow();
            var subject = await subjectService.GetSubjectByIdAsync(id, mentorId);
            return Ok(subject);
        }

        [HttpPost]
        public async Task<ActionResult<SubjectResponseDTO>> CreateSubject(SubjectCreateDTO subjectCreateDTO)
        {
            var mentorId = GetMentorIdOrThrow();
            var result = await subjectService.CreateSubjectAsync(subjectCreateDTO, mentorId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSubject(string id)
        {
            var mentorId = GetMentorIdOrThrow();
            await subjectService.DeleteSubjectAsync(id, mentorId);
            return NoContent();
        }

        [HttpPost("{id}/upload-syllabus")]
        public async Task<ActionResult<IReadOnlyList<TopicResponseDTO>>> UploadSyllabus(string id, [FromForm] IFormFile file)
        {
            var mentorId = GetMentorIdOrThrow();
            var topics = await subjectService.ProcessSyllabusUploadAsync(id, mentorId, file);
            return Ok(topics);
        }

        [HttpGet("{id}/topics")]
        public async Task<ActionResult<IReadOnlyList<TopicResponseDTO>>> GetTopics(string id)
        {
            var mentorId = GetMentorIdOrThrow();
            var topics = await subjectService.GetSubjectTopicsAsync(id, mentorId);
            return Ok(topics);
        }

        [HttpPut("{id}/topics/selection")]
        public async Task<ActionResult<IReadOnlyList<TopicResponseDTO>>> UpdateTopicSelection(string id, UpdateTopicSelectionDTO request)
        {
            var mentorId = GetMentorIdOrThrow();
            var topics = await subjectService.UpdateTopicSelectionAsync(id, mentorId, request);
            return Ok(topics);
        }

        [HttpPost("{id}/generate-quiz")]
        public async Task<ActionResult<QuizGenerationResponseDTO>> GenerateAdaptiveQuiz(string id, AdaptiveQuizRequestDTO request)
        {
            var mentorId = GetMentorIdOrThrow();
            var generatedQuiz = await subjectService.ProcessAdaptiveQuizGenerationAsync(id, mentorId, request);
            return Ok(generatedQuiz);
        }

        [HttpGet("{id}/questions")]
        public async Task<ActionResult<IReadOnlyList<SubjectQuestionDTO>>> GetSubjectQuestions(string id)
        {
            var mentorId = GetMentorIdOrThrow();
            var questions = await subjectService.GetSubjectQuestionsAsync(id, mentorId);
            return Ok(questions);
        }

        [HttpPut("{id}/questions/{questionId}/approval")]
        public async Task<ActionResult<SubjectQuestionDTO>> UpdateQuestionApproval(string id, string questionId, [FromBody] bool isApproved)
        {
            var mentorId = GetMentorIdOrThrow();
            var question = await subjectService.SetQuestionApprovalAsync(id, questionId, mentorId, isApproved);
            return Ok(question);
        }

        [HttpDelete("{id}/questions/{questionId}")]
        public async Task<ActionResult> DiscardQuestion(string id, string questionId)
        {
            var mentorId = GetMentorIdOrThrow();
            await subjectService.DiscardQuestionAsync(id, questionId, mentorId);
            return NoContent();
        }

        private string GetMentorIdOrThrow()
        {
            var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(mentorId))
            {
                throw new UnauthorizedException("Unauthorized");
            }

            return mentorId;
        }
    }
}
