using API.Data;
using API.Entities;
using API.Exceptions;
using API.Subjects.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Subjects.Repositories;

public class SubjectRepository(ApplicationDbContext context) : ISubjectRepository
{
    public async Task<Subject?> GetSubjectByIdAsync(string id)
    {
        return await context.Subjects
            .Include(s => s.Syllabus)
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<Subject>> GetSubjectsByMentorIdAsync(string mentorId)
    {
        return await context.Subjects
            .Include(s => s.Syllabus)
            .Include(s => s.Questions)
            .Where(x => x.MentorId == mentorId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> AddTopicsToSubjects(string subjectId, IEnumerable<Topic> topics)
    {
        var subject = await context.Subjects
            .Include(s => s.Topics)
            .FirstOrDefaultAsync(x => x.Id == subjectId);

        if (subject == null)
        {
            throw new NotFoundException("Invalid request");
        }

        if (subject.Topics.Count > 0)
        {
            context.RemoveRange(subject.Topics);
            subject.Topics.Clear();
        }

        foreach (var topic in topics)
        {
            if (string.IsNullOrEmpty(topic.Id)) topic.Id = Guid.NewGuid().ToString();
            subject.Topics.Add(topic);
        }

        subject.Status = "topics-generated";
        subject.HasSyllabus = true;

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<List<Topic>> GetTopicsBySubjectIdAsync(string subjectId)
    {
        return await context.Set<Topic>()
            .Where(t => t.SubjectId == subjectId)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task SaveTopicSelectionAsync(IEnumerable<Topic> topics)
    {
        context.Set<Topic>().UpdateRange(topics);
        await SaveChangesAsync();
    }

    public void DeleteSubject(Subject subject)
    {
        context.Subjects.Remove(subject);
    }

    public async Task<IEnumerable<string>> GetTopicNamesByIdsAsync(IEnumerable<string> topicIds)
    {
        return await context.Set<Topic>()
            .Where(t => topicIds.Contains(t.Id))
            .Select(t => t.Name)
            .ToListAsync();
    }

    public async Task SaveQuizQuestionsBatchAsync(IEnumerable<Question> questions)
    {
        // EF tracks references. Adding them to the Context attaches them 
        // to your Questions tracking set automatically.
        await context.Set<Question>().AddRangeAsync(questions);
        await SaveChangesAsync();
    }

    public async Task<List<Question>> GetQuestionsBySubjectIdAsync(string subjectId)
    {
        return await context.Set<Question>()
            .Include(q => q.Topic)
            .Include(q => q.Options)
            .Where(q => q.SubjectId == subjectId)
            .OrderByDescending(q => q.IsApproved)
            .ThenBy(q => q.Topic.Name)
            .ToListAsync();
    }

    public async Task<Question?> GetQuestionWithRelationsAsync(string questionId)
    {
        return await context.Set<Question>()
            .Include(q => q.Subject)
            .Include(q => q.Topic)
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId);
    }

    public void DeleteQuestion(Question question)
    {
        context.Set<Question>().Remove(question);
    }

    public async Task<Topic?> GetTopicWithSubjectCheckAsync(string topicId)
    {
        return await context.Set<Topic>()
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.Id == topicId);
    }

    public async Task<List<Topic>> GetTopicsWithSubjectByIdsAsync(IEnumerable<string> topicIds)
    {
        return await context.Set<Topic>()
            .Include(t => t.Subject)
            .Where(t => topicIds.Contains(t.Id))
            .ToListAsync();
    }

    public async Task AddAsync(Subject subject)
    {
        await context.Subjects.AddAsync(subject);
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Database update conflict");
        }
    }
}
