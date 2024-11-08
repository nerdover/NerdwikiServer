using Microsoft.EntityFrameworkCore;
using NerdwikiServer.Data;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Repositories.Interfaces;

namespace NerdwikiServer.Repositories;

public class CategoryRepository(ApplicationDbContext context) : ICategoryRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task Create(Category entity)
    {
        await _context.Categories.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Category entity)
    {
        _context.Categories.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Category>> GetAll()
    {
        return await _context.Categories.AsNoTracking().ToListAsync();
    }

    public async Task<Category?> GetById(string id)
    {
        return await _context.Categories.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
    }

    public async Task Update(Category entity)
    {
        _context.Categories.Update(entity);
        await _context.SaveChangesAsync();
    }
}