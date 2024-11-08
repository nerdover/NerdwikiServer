using Microsoft.EntityFrameworkCore;
using NerdwikiServer.Data;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Repositories.Interfaces;

namespace NerdwikiServer.Repositories;

public class SeriesRepository(ApplicationDbContext context) : ISeriesRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task Create(Series entity)
    {
        await _context.Series.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Series entity)
    {
        _context.Series.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Series>> GetAll()
    {
        return await _context.Series.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Series>> GetByCategoryId(string categoryId)
    {
        return await _context.Series.AsNoTracking().Where(s => s.CategoryId == categoryId).ToListAsync();
    }

    public async Task<Series?> GetById(string id)
    {
        return await _context.Series.AsNoTracking().SingleOrDefaultAsync(s => s.Id == id);
    }

    public async Task Update(Series entity)
    {
        _context.Series.Update(entity);
        await _context.SaveChangesAsync();
    }
}