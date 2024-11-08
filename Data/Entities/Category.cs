using NerdwikiServer.Data.Base;

namespace NerdwikiServer.Data.Entities;

public partial class Category : Entity
{
    public virtual ICollection<Lesson> Lessons { get; set; } = [];
    public virtual ICollection<Series> Series { get; set; } = [];
    public virtual ICollection<SeriesLesson> SeriesLessons { get; set; } = [];
}
