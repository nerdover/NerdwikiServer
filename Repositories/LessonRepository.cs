using Microsoft.EntityFrameworkCore;
using NerdwikiServer.Data;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Repositories.Interfaces;

namespace NerdwikiServer.Repositories;

public class LessonRepository(ApplicationDbContext context) : ILessonRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task Create(Lesson entity)
    {
        await _context.Lessons.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Lesson entity)
    {
        _context.Lessons.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Lesson>> GetAll()
    {
        return await _context.Lessons
            .AsNoTracking()
            .Include(l => l.Category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lesson>> GetByCategoryId(string categoryId)
    {
        return await _context.Lessons
            .AsNoTracking()
            .Include(l => l.Category)
            .Where(l => l.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<Lesson?> GetById(string id)
    {
        return await _context.Lessons
            .AsNoTracking()
            .Include(l => l.Category)
            .SingleOrDefaultAsync(l => l.Id == id);
    }

    public async Task Update(Lesson entity)
    {
        _context.Lessons.Update(entity);
        await _context.SaveChangesAsync();
    }
}