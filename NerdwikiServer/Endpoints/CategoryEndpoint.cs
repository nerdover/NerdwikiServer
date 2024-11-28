using Microsoft.EntityFrameworkCore;
using NerdwikiServer.Data;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Extensions;

namespace NerdwikiServer.Endpoints;

public static class CategoryEndpoint
{
    private record CategoryDto(string Id, string Name);

    public static WebApplication MapCategoriesApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories").RequireAuthorization("AdminAccess");

        group.MapGet("/", GetAllCategories);
        group.MapGet("/{id}", GetCategory);
        group.MapPost("/", CreateCategory);
        group.MapPut("/{id}", UpdateCategory);
        group.MapDelete("/{id}", DeleteCategory);

        return app;
    }

    private static async Task<IResult> GetAllCategories(ApplicationDbContext context)
    {
        var categories = await context.Categories.ToListAsync();
        return TypedResults.Ok(categories);
    }

    private static async Task<IResult> GetCategory(string id, ApplicationDbContext context)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is null)
            return TypedResults.NotFound($"Category with id '{id}' not found.");
        return TypedResults.Ok(category);
    }

    private static async Task<IResult> CreateCategory(CategoryDto dto, ApplicationDbContext context)
    {
        var validationError = ValidateCategoryAsync(dto.Id, dto.Name);
        if (validationError is not null)
            return TypedResults.BadRequest(validationError);

        var normalizedId = dto.Id.NormalizedId();
        if (await context.Categories.AnyAsync(c => c.Id == normalizedId))
            return TypedResults.Conflict($"A category with the id '{normalizedId}' already exists.");

        var normalizedName = dto.Name.NormalizedName();
        if (await context.Categories.AnyAsync(c => c.Name == normalizedName))
            return TypedResults.Conflict($"A category with the name '{normalizedName}' already exists.");

        try
        {
            Category category = new()
            {
                Id = dto.Id.NormalizedId(),
                Name = dto.Name.NormalizedName(),
            };
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            return TypedResults.Created($"{category.Id}", category);
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while saving the category.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static async Task<IResult> UpdateCategory(string id, CategoryDto dto, ApplicationDbContext context)
    {
        if (id != dto.Id)
            return TypedResults.BadRequest("Id in parameter doesn't match id in body.");

        var category = await context.Categories.FindAsync(id);
        if (category is null)
            return TypedResults.NotFound($"Category with id '{id}' not found.");

        var validationError = ValidateCategoryAsync(dto.Id, dto.Name);
        if (validationError is not null)
            return TypedResults.BadRequest(validationError);

        var normalizedName = dto.Name.NormalizedName();
        if (await context.Categories.AnyAsync(c => c.Name == normalizedName))
            return TypedResults.Conflict($"A category with the name '{normalizedName}' already exists.");

        try
        {
            category.Name = dto.Name.NormalizedName();

            context.Categories.Update(category);
            await context.SaveChangesAsync();

            return TypedResults.Ok(category);
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while updating the category.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static async Task<IResult> DeleteCategory(string id, ApplicationDbContext context)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is null)
            return TypedResults.NotFound($"Category with id '{id}' not found.");

        try
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while deleting the category.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static string? ValidateCategoryAsync(string id, string name)
    {
        int maxLength = 50;
        if (string.IsNullOrWhiteSpace(id))
            return "Category id cannot be empty or whitespace.";

        if (string.IsNullOrWhiteSpace(name))
            return "Category name cannot be empty or whitespace.";

        var normalizedId = id.NormalizedId();
        if (normalizedId.Length > maxLength)
            return $"Category id must not exceed {maxLength} characters.";

        var normalizedName = name.NormalizedName();
        if (normalizedName.Length > maxLength)
            return $"Category name must not exceed {maxLength} characters.";

        return null;
    }
}
