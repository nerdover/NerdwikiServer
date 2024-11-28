namespace NerdwikiServer.Data.Entities;

public partial class Lesson
{
    public string Id { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? CategoryId { get; set; }

    public string? TopicId { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Topic? Topic { get; set; }
}
