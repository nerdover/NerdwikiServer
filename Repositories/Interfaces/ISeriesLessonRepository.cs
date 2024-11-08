using NerdwikiServer.Data.Entities;

namespace NerdwikiServer.Repositories.Interfaces;

public interface ISeriesLessonRepository : IRepository<SeriesLesson, string>
{
    Task<IEnumerable<SeriesLesson>> GetByCategoryId(string categoryId);
    Task<IEnumerable<SeriesLesson>> GetBySeriesId(string seriesId);
}