namespace API.Subjects.DTO;

public class SubjectCreateDTO
{
    public required string SubjectName { get; set; }
    public string Description { get; set; } = string.Empty;
}

