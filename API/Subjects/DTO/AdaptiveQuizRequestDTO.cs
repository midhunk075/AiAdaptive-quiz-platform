namespace API.Subjects.DTO
{
    public class AdaptiveQuizRequestDTO
    {
        public List<string> SelectedTopicIds { get; set; } = new();
        public double? UserPreviousScorePercentage { get; set; }
        public int TotalQuestionCount { get; set; } = 5;
        public bool GenerateAllDifficultyLevels { get; set; } = true;
    }
}
