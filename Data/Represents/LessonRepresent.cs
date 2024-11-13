namespace NerdwikiServer.Data.Represents;

public class LessonRepresent
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? CategoryId { get; set; }
    public string? CategoryTitle { get; set; }
    public string? Cover { get; set; }
    public string? Hex { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}