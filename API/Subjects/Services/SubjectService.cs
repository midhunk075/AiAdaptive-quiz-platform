using API.Entities;
using API.Exceptions;
using API.Subjects.DTO;
using API.Subjects.Interfaces;

namespace API.Subjects.Services
{
    public class SubjectService(ISubjectRepository subjectRepository, ISyllabusService syllabusService,IQuestionGeneratorService questionGeneratorService) : ISubjectService
    {
        public async Task<SubjectResponseDTO> CreateSubjectAsync(SubjectCreateDTO subjectCreateDTO, string mentorId)
        {
            var subject = new Subject
            {
                SubjectName = subjectCreateDTO.SubjectName,
                Description = subjectCreateDTO.Description,
                MentorId = mentorId,
                Status = "Draft",
            };

            await subjectRepository.AddAsync(subject);
            await subjectRepository.SaveChangesAsync();

            return new SubjectResponseDTO
            {
                Id = subject.Id,
                Description = subject.Description ?? string.Empty,
                SubjectName = subject.SubjectName,
                Status = subject.Status,
                QuestionCount = 0,
                HasSyllabus = false
            };
        }

        public async Task DeleteSubjectAsync(string subjectId, string mentorId)
        {
            var subject = await subjectRepository.GetSubjectByIdAsync(subjectId);

            if (subject == null || subject.MentorId != mentorId)
            {
                throw new NotFoundException("Invalid Request");
            }

            subjectRepository.DeleteSubject(subject);
            await subjectRepository.SaveChangesAsync();

        }

        public async Task<IEnumerable<SubjectResponseDTO>> GetMentorSubjectsAsync(string mentorId)
        {
            var subjects = await subjectRepository.GetSubjectsByMentorIdAsync(mentorId);

            return subjects.Select(s => new SubjectResponseDTO
            {
                Id = s.Id,
                SubjectName = s.SubjectName,
                Description = s.Description ?? string.Empty,
                Status = s.Status,
                QuestionCount = s.Questions?.Count() ?? 0,
                HasSyllabus = s.HasSyllabus || s.Syllabus != null
            });
        }

        public async Task<SubjectResponseDTO> GetSubjectByIdAsync(string subjectId, string mentorId)
        {
            var subject = await subjectRepository.GetSubjectByIdAsync(subjectId);

            if (subject == null || subject.MentorId != mentorId)
            {
                throw new NotFoundException("Invalid request");
            }

            return new SubjectResponseDTO
            {
                Id = subject.Id,
                SubjectName = subject.SubjectName,
                Description = subject.Description ?? string.Empty,
                Status = subject.Status,
                QuestionCount = subject.Questions?.Count() ?? 0,
                HasSyllabus = subject.HasSyllabus || subject.Syllabus != null
            };
        }

        public async Task<IReadOnlyList<TopicResponseDTO>> ProcessSyllabusUploadAsync(string subjectId, string mentorId, IFormFile file)
        {
            var subject = await subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null || subject.MentorId != mentorId)
            {
                throw new NotFoundException("Invalid request");
            }

            var rawText = await syllabusService.ExtractTextAsync(file);
            var topicNames = await syllabusService.ExtractTopicsWithAIAsync(rawText);

            var normalizedTopicNames = topicNames
                .Select(name => name?.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalizedTopicNames.Count == 0)
            {
                throw new ValidationException("No topics could be extracted from the uploaded syllabus.");
            }

            var topicEntities = normalizedTopicNames.Select(name => new Topic
            {
                Name = name!,
                SubjectId = subjectId,
                IsSelected = true
            }).ToList();

            await subjectRepository.AddTopicsToSubjects(subjectId, topicEntities);
            return ToTopicResponse(topicEntities);
        }

        public async Task<IReadOnlyList<TopicResponseDTO>> GetSubjectTopicsAsync(string subjectId, string mentorId)
        {
            var subject = await subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null || subject.MentorId != mentorId)
            {
                throw new NotFoundException("Invalid request");
            }

            var topics = await subjectRepository.GetTopicsBySubjectIdAsync(subjectId);
            return ToTopicResponse(topics);
        }

        public async Task<IReadOnlyList<TopicResponseDTO>> UpdateTopicSelectionAsync(string subjectId, string mentorId, UpdateTopicSelectionDTO request)
        {
            var subject = await subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null || subject.MentorId != mentorId)
            {
                throw new NotFoundException("Invalid request");
            }

            if (request.Topics.Count == 0)
            {
                throw new ValidationException("At least one topic update is required.");
            }

            var topics = await subjectRepository.GetTopicsBySubjectIdAsync(subjectId);
            var topicMap = topics.ToDictionary(t => t.Id, StringComparer.Ordinal);

            foreach (var update in request.Topics)
            {
                if (!topicMap.TryGetValue(update.TopicId, out var topic))
                {
                    throw new NotFoundException($"Topic '{update.TopicId}' was not found for this subject.");
                }

                topic.IsSelected = update.IsSelected;
            }

            await subjectRepository.SaveTopicSelectionAsync(topics);
            return ToTopicResponse(topics);
        }

        public async Task<QuizGenerationResponseDTO> ProcessAdaptiveQuizGenerationAsync(string subjectId, string mentorId, AdaptiveQuizRequestDTO request)
        {
            if (request.SelectedTopicIds == null || !request.SelectedTopicIds.Any())
            {
                throw new ValidationException("Please select at least one topic to generate a quiz.");
            }

            var subject = await subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null || subject.MentorId != mentorId)
            {
                throw new NotFoundException("Subject not found or access is unauthorized.");
            }

            var selectedTopicIds = request.SelectedTopicIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            if (selectedTopicIds.Count == 0)
            {
                throw new ValidationException("Please provide valid topic identifiers.");
            }

            var selectedTopics = await subjectRepository.GetTopicsWithSubjectByIdsAsync(selectedTopicIds);
            if (selectedTopics.Count != selectedTopicIds.Count)
            {
                throw new NotFoundException("One or more selected topics were not found.");
            }

            if (selectedTopics.Any(t => t.SubjectId != subjectId || t.Subject.MentorId != mentorId))
            {
                throw new ForbiddenException("One or more selected topics do not belong to this subject.");
            }

            int targetDifficultyLevel;
            if (!request.UserPreviousScorePercentage.HasValue)
            {
                targetDifficultyLevel = 2;
            }
            else
            {
                double score = request.UserPreviousScorePercentage.Value;
                targetDifficultyLevel = score switch
                {
                    > 80 => 3,
                    < 40 => 1,
                    _ => 2
                };
            }

            if (request.TotalQuestionCount <= 0)
            {
                throw new ValidationException("TotalQuestionCount must be greater than zero.");
            }

            var difficultyLevels = request.GenerateAllDifficultyLevels
                ? new[] { 1, 2, 3 }
                : new[] { targetDifficultyLevel };

            var generatedQuestions = new List<(GeneratedQuestionDTO Question, int DifficultyLevel)>();
            var topicContext = string.Join(", ", selectedTopics.Select(t => t.Name));

            foreach (var difficultyLevel in difficultyLevels)
            {
                var generatedQuizDto = await questionGeneratorService.GenerateQuestionsAsync(
                    topicName: topicContext,
                    difficultyLevel: difficultyLevel,
                    count: request.TotalQuestionCount
                );

                if (generatedQuizDto.Questions == null || generatedQuizDto.Questions.Count == 0)
                {
                    throw new ValidationException($"AI returned no questions for difficulty level {difficultyLevel}.");
                }

                generatedQuestions.AddRange(generatedQuizDto.Questions.Select(q => (q, difficultyLevel)));
            }

            if (generatedQuestions.Count == 0)
            {
                throw new ValidationException("AI returned no questions.");
            }

            var orderedTopics = selectedTopics.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
            var questionEntities = new List<Question>();
            for (var i = 0; i < generatedQuestions.Count; i++)
            {
                var (q, difficultyLevel) = generatedQuestions[i];
                if (string.IsNullOrWhiteSpace(q.QuestionText))
                {
                    throw new ValidationException("AI returned a question with empty text.");
                }

                var options = q.Options
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (options.Count != 4)
                {
                    throw new ValidationException("Each generated question must contain exactly 4 unique options.");
                }
                if (string.IsNullOrWhiteSpace(q.CorrectAnswer) || !options.Contains(q.CorrectAnswer.Trim(), StringComparer.OrdinalIgnoreCase))
                {
                    throw new ValidationException("Each generated question must include a valid correct answer present in options.");
                }

                var assignedTopic = orderedTopics[i % orderedTopics.Count];
                questionEntities.Add(new Question
                {
                    Id = Guid.NewGuid().ToString(),
                    QuestionText = q.QuestionText.Trim(),
                    DifficultyLevel = difficultyLevel,
                    TopicId = assignedTopic.Id,
                    SubjectId = subject.Id,
                    IsApproved = false,
                    CorrectAnswer = q.CorrectAnswer.Trim(),
                    Explanation = q.Explanation?.Trim() ?? string.Empty,
                    Options = options.Select(opt => new Option
                    {
                        Id = Guid.NewGuid().ToString(),
                        OptionText = opt
                    }).ToList()
                });
            }

            subject.Status = "questions-generated";
            await subjectRepository.SaveQuizQuestionsBatchAsync(questionEntities);
            return new QuizGenerationResponseDTO
            {
                Questions = generatedQuestions.Select(q => q.Question).ToList()
            };
        }

        public async Task<IReadOnlyList<SubjectQuestionDTO>> GetSubjectQuestionsAsync(string subjectId, string mentorId)
        {
            var subject = await subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null || subject.MentorId != mentorId)
            {
                throw new NotFoundException("Subject not found or access is unauthorized.");
            }

            var questions = await subjectRepository.GetQuestionsBySubjectIdAsync(subjectId);
            return questions.Select(MapQuestionToDto).ToList();
        }

        public async Task<SubjectQuestionDTO> SetQuestionApprovalAsync(string subjectId, string questionId, string mentorId, bool isApproved)
        {
            var question = await subjectRepository.GetQuestionWithRelationsAsync(questionId);
            if (question == null || question.SubjectId != subjectId || question.Subject.MentorId != mentorId)
            {
                throw new NotFoundException("Question not found or access is unauthorized.");
            }

            question.IsApproved = isApproved;
            await subjectRepository.SaveChangesAsync();

            return MapQuestionToDto(question);
        }

        public async Task DiscardQuestionAsync(string subjectId, string questionId, string mentorId)
        {
            var question = await subjectRepository.GetQuestionWithRelationsAsync(questionId);
            if (question == null || question.SubjectId != subjectId || question.Subject.MentorId != mentorId)
            {
                throw new NotFoundException("Question not found or access is unauthorized.");
            }

            subjectRepository.DeleteQuestion(question);
            await subjectRepository.SaveChangesAsync();
        }

        private static SubjectQuestionDTO MapQuestionToDto(Question q)
        {
            return new SubjectQuestionDTO
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                DifficultyLevel = q.DifficultyLevel,
                IsApproved = q.IsApproved,
                TopicId = q.TopicId,
                TopicName = q.Topic?.Name ?? string.Empty,
                CorrectAnswer = q.CorrectAnswer,
                Explanation = q.Explanation,
                Options = q.Options.Select(o => o.OptionText).ToList()
            };
        }

        private static List<TopicResponseDTO> ToTopicResponse(IEnumerable<Topic> topics)
        {
            return topics
                .OrderBy(t => t.Name)
                .Select(t => new TopicResponseDTO
                {
                    Id = t.Id,
                    Name = t.Name,
                    IsSelected = t.IsSelected
                })
                .ToList();
        }
    }
}
