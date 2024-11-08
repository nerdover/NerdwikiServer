using NerdwikiServer.Data.Base;

namespace NerdwikiServer.Data.Entities;

public partial class Lesson : Entity
{
    public string CategoryId { get; set; } = null!;
    public string? Content { get; set; }
    public virtual Category Category { get; set; } = null!;
}
