namespace NerdwikiServer.Data.Entities;

public partial class Category
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
