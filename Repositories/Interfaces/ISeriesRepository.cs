using NerdwikiServer.Data.Entities;

namespace NerdwikiServer.Repositories.Interfaces;

public interface ISeriesRepository : IRepository<Series, string>
{
    Task<IEnumerable<Series>> GetByCategoryId(string categoryId);
}