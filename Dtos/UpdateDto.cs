namespace NerdwikiServer.Dtos;

public record UpdateCategoryDto(string Id, string? Title, string? Cover, string? Hex);
public record UpdateLessonDto(string Id, string? Title, string? Cover, string? Hex, string? Content);
public record UpdateSeriesDto(string Id, string? Title, string? Cover, string? Hex);
public record UpdateSeriesLessonDto(string Id, string? Title, string? Cover, string? Hex, string? Content);