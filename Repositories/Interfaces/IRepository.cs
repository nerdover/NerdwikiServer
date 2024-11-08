namespace NerdwikiServer.Repositories.Interfaces;

public interface IRepository<T, K>
{
    Task<IEnumerable<T>> GetAll();
    Task<T?> GetById(K id);
    Task Create(T entity);
    Task Update(T entity);
    Task Delete(T entity);
}