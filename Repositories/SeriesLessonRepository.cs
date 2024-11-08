using Microsoft.EntityFrameworkCore;
using NerdwikiServer.Data;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Repositories.Interfaces;

namespace NerdwikiServer.Repositories;

public class SeriesLessonRepository(ApplicationDbContext context) : ISeriesLessonRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task Create(SeriesLesson entity)
    {
        await _context.SeriesLessons.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(SeriesLesson entity)
    {
        _context.SeriesLessons.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<SeriesLesson>> GetAll()
    {
        return await _context.SeriesLessons.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<SeriesLesson>> GetByCategoryId(string categoryId)
    {
        return await _context.SeriesLessons
            .AsNoTracking()
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<SeriesLesson?> GetById(string id)
    {
        return await _context.SeriesLessons.AsNoTracking().SingleOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<SeriesLesson>> GetBySeriesId(string seriesId)
    {
        return await _context.SeriesLessons
            .AsNoTracking()
            .Where(x => x.SeriesId == seriesId)
            .ToListAsync();
    }

    public async Task Update(SeriesLesson entity)
    {
        _context.SeriesLessons.Update(entity);
        await _context.SaveChangesAsync();
    }
}