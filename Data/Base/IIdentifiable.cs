namespace NerdwikiServer.Data.Base;

public interface IIdentifiable
{
    public string Id { get; set; }
    public string Title { get; set; }
}