using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NerdwikiServer.Data.Base;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Dtos;
using NerdwikiServer.Repositories.Interfaces;

namespace NerdwikiServer.Controllers;

[ApiController]
[Route("[controller]")]
public class LessonsController(ILessonRepository lessonRepository, ICategoryRepository categoryRepository) : ControllerBase
{
    private readonly ILessonRepository _lessonRepository = lessonRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    [HttpPost]
    public async Task<IActionResult> CreateLesson(CreateLessonDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Id))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Id is required" });
            }

            if (string.IsNullOrEmpty(dto.Title))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Title is required" });
            }

            var foundLesson = await _lessonRepository.GetById(dto.Id);

            if (foundLesson is not null)
            {
                return Conflict(new ServerResponse { Success = false, Message = "Lesson is already in the repository" });
            }

            Lesson lesson = new()
            {
                Id = dto.Id,
                CategoryId = dto.CategoryId,
                Title = dto.Title,
                Cover = dto.Cover,
                Hex = dto.Hex,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            await _lessonRepository.Create(lesson);

            return Created();
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLesson(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Id is required" });
            }

            var foundLesson = await _lessonRepository.GetById(id);

            if (foundLesson is null)
            {
                return NotFound(new ServerResponse { Success = false, Message = "Lesson does not exist" });
            }

            await _lessonRepository.Delete(foundLesson);

            return NoContent();
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Lesson>>> GetLessons()
    {
        try
        {
            var lessons = await _lessonRepository.GetAll();

            return Ok(new ServerResponse<IEnumerable<Lesson>>() { Success = true, Data = lessons });
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<Lesson>> GetLessonById(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Id is required" });
            }

            var lesson = await _lessonRepository.GetById(id);

            if (lesson is null)
            {
                return NotFound(new ServerResponse { Success = false, Message = "Lesson does not exist" });
            }

            return Ok(new ServerResponse<Lesson>() { Success = true, Data = lesson });
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [AllowAnonymous]
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<Lesson>>> GetLessonsByCategoryId(string categoryId)
    {
        try
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Category Id is required" });
            }

            var category = await _categoryRepository.GetById(categoryId);

            if (category is null)
            {
                return NotFound(new ServerResponse { Success = false, Message = "Category does not exist" });
            }

            var lessons = await _lessonRepository.GetByCategoryId(categoryId);

            return Ok(new ServerResponse<IEnumerable<Lesson>>() { Success = true, Data = lessons });
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLesson(string id, UpdateLessonDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Id is required" });
            }

            if (id != dto.Id)
            {
                return BadRequest(new ServerResponse() { Success = false, Message = "Id in the request body does not match the id in the URL" });
            }

            var foundLesson = await _lessonRepository.GetById(id);

            if (foundLesson is null)
            {
                return NotFound(new ServerResponse() { Success = false, Message = "Lesson does not exist" });
            }

            Lesson lesson = new()
            {
                Id = foundLesson.Id,
                CategoryId = foundLesson.CategoryId,
                Title = dto.Title ?? foundLesson.Title,
                Cover = dto.Cover ?? foundLesson.Cover,
                Hex = dto.Hex ?? foundLesson.Hex,
                Content = dto.Content ?? foundLesson.Content,
                CreatedAt = foundLesson.CreatedAt,
                UpdatedAt = DateTime.Now,
            };

            await _lessonRepository.Update(lesson);

            return NoContent();
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }
}