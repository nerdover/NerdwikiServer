using Microsoft.EntityFrameworkCore;
using NerdwikiServer.Data;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Extensions;

namespace NerdwikiServer.Endpoints;

public static class TopicEndpoint
{
    private record TopicDto(string Id, string CategoryId, string Name);

    public static WebApplication MapTopicsApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/topics").RequireAuthorization("AdminAccess");

        group.MapGet("/", GetAllTopics).AllowAnonymous();
        group.MapGet("/{id}", GetTopic);
        group.MapPost("/", CreateTopic);
        group.MapPut("/{id}", UpdateTopic);
        group.MapDelete("/{id}", DeleteTopic);

        return app;
    }

    private static async Task<IResult> GetAllTopics(ApplicationDbContext context)
    {
        var topics = await context.Topics.ToListAsync();
        return TypedResults.Ok(topics);
    }

    private static async Task<IResult> GetTopic(string id, ApplicationDbContext context)
    {
        var topic = await context.Topics.FindAsync(id);
        if (topic is null)
            return TypedResults.NotFound($"Topic with id '{id}' not found.");
        return TypedResults.Ok(topic);
    }

    private static async Task<IResult> CreateTopic(TopicDto dto, ApplicationDbContext context)
    {
        var validationError = ValidateTopicAsync(dto.Id, dto.Name);
        if (validationError is not null)
            return TypedResults.BadRequest(validationError);

        var categoryExist = await context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExist)
            return TypedResults.NotFound($"Category with id '{dto.CategoryId}' not found.");

        var normalizedId = dto.Id.NormalizedId();
        if (await context.Topics.AnyAsync(t => t.Id == normalizedId))
            return TypedResults.Conflict($"A topic with the id '{normalizedId}' already exists.");

        var normalizedName = dto.Name.NormalizedName();
        if (await context.Topics.AnyAsync(t => t.Name == normalizedName))
            return TypedResults.Conflict($"A topic with the name '{normalizedName}' already exists.");

        try
        {
            Topic topic = new()
            {
                Id = dto.Id.NormalizedId(),
                CategoryId = dto.CategoryId,
                Name = dto.Name.NormalizedName(),
            };
            await context.Topics.AddAsync(topic);
            await context.SaveChangesAsync();

            return TypedResults.Created($"{topic.Id}", topic);
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while saving the topic.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static async Task<IResult> UpdateTopic(string id, TopicDto dto, ApplicationDbContext context)
    {
        if (id != dto.Id)
            return TypedResults.BadRequest("Id in parameter doesn't match id in body.");

        var topic = await context.Topics.FindAsync(id);
        if (topic is null)
            return TypedResults.NotFound($"Topic with id '{id}' not found.");

        var validationError = ValidateTopicAsync(dto.Id, dto.Name);
        if (validationError is not null)
            return TypedResults.BadRequest(validationError);

        var normalizedName = dto.Name.NormalizedName();
        if (await context.Topics.AnyAsync(t => t.Name == normalizedName))
            return TypedResults.Conflict($"A topic with the name '{normalizedName}' already exists.");

        try
        {
            topic.Name = dto.Name.NormalizedName();

            context.Topics.Update(topic);
            await context.SaveChangesAsync();

            return TypedResults.Ok(topic);
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while updating the topic.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static async Task<IResult> DeleteTopic(string id, ApplicationDbContext context)
    {
        var topic = await context.Topics.FindAsync(id);
        if (topic is null)
            return TypedResults.NotFound($"Topic with id '{id}' not found.");

        try
        {
            context.Topics.Remove(topic);
            await context.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while deleting the topic.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static string? ValidateTopicAsync(string id, string name)
    {
        int maxLength = 50;
        if (string.IsNullOrWhiteSpace(id))
            return "Topic id cannot be empty or whitespace.";

        if (string.IsNullOrWhiteSpace(name))
            return "Topic name cannot be empty or whitespace.";

        var normalizedId = id.NormalizedId();
        if (normalizedId.Length > maxLength)
            return $"Topic id must not exceed {maxLength} characters.";

        var normalizedName = name.NormalizedName();
        if (normalizedName.Length > maxLength)
            return $"Topic name must not exceed {maxLength} characters.";

        return null;
    }
}
