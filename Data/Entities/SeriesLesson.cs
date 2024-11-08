using NerdwikiServer.Data.Base;

namespace NerdwikiServer.Data.Entities;

public partial class SeriesLesson : Entity
{
    public string CategoryId { get; set; } = null!;
    public string SeriesId { get; set; } = null!;
    public string? Content { get; set; }
    public virtual Category Category { get; set; } = null!;
    public virtual Series Series { get; set; } = null!;
}
