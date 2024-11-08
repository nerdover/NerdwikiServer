namespace NerdwikiServer.Data.Base;

public class ServerResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class ServerResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}