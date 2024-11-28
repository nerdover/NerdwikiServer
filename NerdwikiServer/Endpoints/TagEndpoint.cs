using Microsoft.EntityFrameworkCore;
using NerdwikiServer.Data;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Extensions;

namespace NerdwikiServer.Endpoints;

public static class TagEndpoint
{
    private record TagDto(string Id, string Name);

    public static WebApplication MapTagsApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/tags").RequireAuthorization("AdminAccess");

        group.MapGet("/", GetAllTags);
        group.MapGet("/{id}", GetTag);
        group.MapPost("/", CreateTag);
        group.MapPut("/{id}", UpdateTag);
        group.MapDelete("/{id}", DeleteTag);

        return app;
    }

    private static async Task<IResult> GetAllTags(ApplicationDbContext context)
    {
        var tags = await context.Tags.ToListAsync();
        return TypedResults.Ok(tags);
    }

    private static async Task<IResult> GetTag(string id, ApplicationDbContext context)
    {
        var tag = await context.Tags.FindAsync(id);
        if (tag is null)
            return TypedResults.NotFound($"Tag with id '{id}' not found.");
        return TypedResults.Ok(tag);
    }

    private static async Task<IResult> CreateTag(TagDto dto, ApplicationDbContext context)
    {
        var validationError = ValidateTagAsync(dto.Id, dto.Name);
        if (validationError is not null)
            return TypedResults.BadRequest(validationError);

        var normalizedId = dto.Id.NormalizedId();
        if (await context.Tags.AnyAsync(c => c.Id == normalizedId))
            return TypedResults.Conflict($"A tag with the id '{normalizedId}' already exists.");

        var normalizedName = dto.Name.NormalizedName();
        if (await context.Tags.AnyAsync(c => c.Name == normalizedName))
            return TypedResults.Conflict($"A tag with the name '{normalizedName}' already exists.");

        try
        {
            Tag tag = new()
            {
                Id = dto.Id.NormalizedId(),
                Name = dto.Name.NormalizedName(),
            };
            await context.Tags.AddAsync(tag);
            await context.SaveChangesAsync();

            return TypedResults.Created($"{tag.Id}", tag);
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while saving the tag.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static async Task<IResult> UpdateTag(string id, TagDto dto, ApplicationDbContext context)
    {
        if (id != dto.Id)
            return TypedResults.BadRequest("Id in parameter doesn't match id in body.");

        var tag = await context.Tags.FindAsync(id);
        if (tag is null)
            return TypedResults.NotFound($"Tag with id '{id}' not found.");

        var validationError = ValidateTagAsync(dto.Id, dto.Name);
        if (validationError is not null)
            return TypedResults.BadRequest(validationError);

        var normalizedName = dto.Name.NormalizedName();
        if (await context.Tags.AnyAsync(c => c.Name == normalizedName))
            return TypedResults.Conflict($"A tag with the name '{normalizedName}' already exists.");

        try
        {
            tag.Name = dto.Name.NormalizedName();

            context.Tags.Update(tag);
            await context.SaveChangesAsync();

            return TypedResults.Ok(tag);
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while updating the tag.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static async Task<IResult> DeleteTag(string id, ApplicationDbContext context)
    {
        var tag = await context.Tags.FindAsync(id);
        if (tag is null)
            return TypedResults.NotFound($"Tag with id '{id}' not found.");

        try
        {
            context.Tags.Remove(tag);
            await context.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        catch (DbUpdateException)
        {
            return TypedResults.Conflict("An error occurred while deleting the tag.");
        }
        catch (Exception)
        {
            return TypedResults.Problem("An unexpected error occurred.");
        }
    }

    private static string? ValidateTagAsync(string id, string name)
    {
        int maxLength = 50;
        if (string.IsNullOrWhiteSpace(id))
            return "Tag id cannot be empty or whitespace.";

        if (string.IsNullOrWhiteSpace(name))
            return "Tag name cannot be empty or whitespace.";

        var normalizedId = id.NormalizedId();
        if (normalizedId.Length > maxLength)
            return $"Tag id must not exceed {maxLength} characters.";

        var normalizedName = name.NormalizedName();
        if (normalizedName.Length > maxLength)
            return $"Tag name must not exceed {maxLength} characters.";

        return null;
    }
}
