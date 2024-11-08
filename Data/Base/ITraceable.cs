namespace NerdwikiServer.Data.Base;

public interface ITraceable
{
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}