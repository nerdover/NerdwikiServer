namespace NerdwikiServer.Data.Base;

public class BaseEntity : IIdentifiable, ITraceable
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}