using NerdwikiServer.Data.Base;

namespace NerdwikiServer.Data.Entities;

public partial class Series : Entity
{
    public string CategoryId { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<SeriesLesson> SeriesLessons { get; set; } = [];
}
