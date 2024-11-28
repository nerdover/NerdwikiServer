namespace NerdwikiServer.Data.Entities;

public partial class Topic
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string CategoryId { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
