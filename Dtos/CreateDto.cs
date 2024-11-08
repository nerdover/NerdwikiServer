namespace NerdwikiServer.Dtos;

public record CreateCategoryDto(string Id, string Title, string? Cover, string? Hex);
public record CreateLessonDto(string Id, string CategoryId, string Title, string? Cover, string? Hex);
public record CreateSeriesDto(string Id, string CategoryId, string Title, string? Cover, string? Hex);
public record CreateSeriesLessonDto(string Id, string SeriesId, string CategoryId, string Title, string? Cover, string? Hex);