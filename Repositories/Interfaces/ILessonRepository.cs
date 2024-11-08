using NerdwikiServer.Data.Entities;

namespace NerdwikiServer.Repositories.Interfaces;

public interface ILessonRepository : IRepository<Lesson, string>
{
    Task<IEnumerable<Lesson>> GetByCategoryId(string categoryId);
}