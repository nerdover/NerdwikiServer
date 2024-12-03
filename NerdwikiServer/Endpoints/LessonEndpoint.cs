using Microsoft.EntityFrameworkCore;
using NerdwikiServer.Data;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Extensions;

namespace NerdwikiServer.Endpoints;

public static class LessonEndpoint
{
    private record CreateLessonDto(string Id, string Title, string? Description, string? CategoryId, string? TopicId, string? Cover);
    private record UpdateLessonDto(string Id, string Title, string? Description, string? Cover);
    private record UpdateContentDto(string Id, string Content);

    public static WebApplication MapLessonsApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/lessons").RequireAuthorization("AdminAccess");

        group.MapGet("/", GetAllLessonsInCategory).AllowAnonymous();
        group.MapGet("/{id}", GetLesson).AllowAnonymous();
        group.MapPost("/", CreateLesson);
        group.MapPut("/{id}", UpdateLesson);
        group.MapPatch("/{id}/content", UpdateContent);
        group.MapDelete("/{id}", DeleteLesson);

        return app;
    }

    private static async Task<IResult> GetAllLessonsInCategory(string? categoryId, ApplicationDbContext context)
    {
        if (categoryId is not null)
        {
            var lessons = await context.Lessons
                .AsNoTracking()
                .Where(l => l.CategoryId == categoryId)
                .Include(l => l.Category)
                .Include(l => l.Topic)
                .Select(l => new Lesson
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    Cover = l.Cover,
                    CategoryId = l.CategoryId,
                    Category = l.Category,
                    TopicId = l.TopicId,
                    Topic = l.Topic
                })
                .ToListAsync();
            return TypedResults.Ok(lessons);
        }

        return TypedResults.Ok(await context.Lessons
            .Include(l => l.Category)
            .Include(l => l.Topic)
            .Select(l => new Lesson
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description,
                Cover = l.Cover,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                CategoryId = l.CategoryId,
                Category = l.Category,
                TopicId = l.TopicId,
                Topic = l.Topic
            })
            .ToListAsync());
    }

    private static async Task<IResult> GetLesson(string id, ApplicationDbContext context)
    {
        var lesson = await context.Lessons
            .Include(l => l.Category)
            .Include(l => l.Topic)
            .Select(l => new Lesson
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description,
                Cover = l.Cover,
                Content = l.Content,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                CategoryId = l.CategoryId,
                Category = l.Category,
                TopicId = l.TopicId,
                Topic = l.Topic
            })
            .SingleOrDefaultAsync(l => l.Id == id);
        if (lesson is null)
            return TypedResults.NotFound($"Lesson with id '{id}' not found.");
        return TypedResults.Ok(lesson);
    }

    private static async Task<IResult> CreateLesson(CreateLessonDto dto, ApplicationDbContext context)
    {
        var validationError = ValidateLessonAsync(dto.Id, dto.Title);
        if (validationError is not null)
            return TypedResults.BadRequest(validationError);

        var normalizedId = dto.Id.NormalizedId();
        if (await context.Lessons.AnyAsync(l => l.Id == normalizedId))
            return TypedResults.Conflict($"A lesson with the id '{normalizedId}' already exists.");

        var normalizedName = dto.Title.NormalizedName();
        if (await context.Lessons.AnyAsync(c => c.Title == normalizedName))
            return TypedResults.Conflict($"A lesson with the title '{normalizedName}' already exists.");

        try
        {
            Lesson lesson = new()
            {
                Id = dto.Id.NormalizedId(),
                Title = dto.Title.NormalizedName(),
                Content = "",
                Description = dto.Description,
                Cover = dto.Cover,
                CreatedAt = DateTime.Now
            };

            if (dto.CategoryId is not null)
            {
                var categoryExist = await context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
                if (!categoryExist)
                    return TypedResults.NotFound($"Category with id '{dto.CategoryId}' not found.");
                lesson.CategoryId = dto.CategoryId;
            }

            if (dto.TopicId is not null)
            {
                var topicExist = await context.Topics.AnyAsync(t => t.Id == dto.TopicId);
                if (!topicExist)
                    return TypedResults.NotFound($"Topic with id '{dto.TopicId}' not found.");
                lesson.TopicId = dto.TopicId;
            }

            await context.Lessons.AddAsync(lesson);
            await context.SaveChangesAsync();

            return TypedResults.Created($"{lesson.Id}", lesson);
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while saving the lesson.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static async Task<IResult> UpdateLesson(string id, UpdateLessonDto dto, ApplicationDbContext context)
    {
        if (id != dto.Id)
            return TypedResults.BadRequest("Id in parameter doesn't match id in body.");

        var lesson = await context.Lessons.FindAsync(id);
        if (lesson is null)
            return TypedResults.NotFound($"Lesson with id '{id}' not found.");

        var validationError = ValidateLessonAsync(dto.Id, dto.Title);
        if (validationError is not null)
            return TypedResults.BadRequest(validationError);

        var normalizedName = dto.Title.NormalizedName();
        if (await context.Lessons.AnyAsync(l => l.Title == normalizedName && l.Id != dto.Id))
            return TypedResults.Conflict($"A lesson with the name '{normalizedName}' already exists.");

        try
        {
            lesson.Title = dto.Title.NormalizedName();
            lesson.Description = dto.Description;
            lesson.Cover = dto.Cover;
            lesson.UpdatedAt = DateTime.Now;

            context.Lessons.Update(lesson);
            await context.SaveChangesAsync();

            return TypedResults.Ok(lesson);
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while updating the lesson.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static async Task<IResult> UpdateContent(string id, UpdateContentDto dto, ApplicationDbContext context)
    {
        if (id != dto.Id)
            return TypedResults.BadRequest("Id in parameter doesn't match id in body.");

        var lesson = await context.Lessons.FindAsync(id);
        if (lesson is null)
            return TypedResults.NotFound($"Lesson with id '{id}' not found.");

        try
        {
            lesson.Content = dto.Content;
            lesson.UpdatedAt = DateTime.Now;

            context.Lessons.Update(lesson);
            await context.SaveChangesAsync();

            return TypedResults.Ok(lesson);
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while updating the lesson.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static async Task<IResult> DeleteLesson(string id, ApplicationDbContext context)
    {
        var lesson = await context.Lessons.FindAsync(id);
        if (lesson is null)
            return TypedResults.NotFound($"Lesson with id '{id}' not found.");

        try
        {
            context.Lessons.Remove(lesson);
            await context.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while deleting the lesson.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static string? ValidateLessonAsync(string id, string title)
    {
        int maxLength = 50;
        if (string.IsNullOrWhiteSpace(id))
            return "Lesson id cannot be empty or whitespace.";

        if (string.IsNullOrWhiteSpace(title))
            return "Lesson title cannot be empty or whitespace.";

        var normalizedId = id.NormalizedId();
        if (normalizedId.Length > maxLength)
            return $"Lesson id must not exceed {maxLength} characters.";

        var normalizedName = title.NormalizedName();
        if (normalizedName.Length > maxLength)
            return $"Lesson title must not exceed {maxLength} characters.";

        return null;
    }
}
