using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NerdwikiServer.Data.Base;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Data.Represents;
using NerdwikiServer.Dtos;
using NerdwikiServer.Repositories.Interfaces;

namespace NerdwikiServer.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SeriesLessonsController(ISeriesLessonRepository seriesLessonRepository, ISeriesRepository seriesRepository, ICategoryRepository categoryRepository) : ControllerBase
{
    private readonly ISeriesLessonRepository _seriesLessonRepository = seriesLessonRepository;
    private readonly ISeriesRepository _seriesRepository = seriesRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    [HttpPost]
    public async Task<ActionResult<IEnumerable<SeriesLesson>>> CreateSeriesLesson(CreateSeriesLessonDto dto)
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

            var foundSeriesLesson = await _seriesLessonRepository.GetById(dto.Id);

            if (foundSeriesLesson is not null)
            {
                return Conflict(new ServerResponse { Success = false, Message = "Series Lesson is already in the repository" });
            }

            SeriesLesson seriesLesson = new()
            {
                Id = dto.Id,
                CategoryId = dto.CategoryId,
                SeriesId = dto.SeriesId,
                Title = dto.Title,
                Cover = dto.Cover,
                Hex = dto.Hex,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            await _seriesLessonRepository.Create(seriesLesson);

            return Created();
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSeriesLesson(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Id is required" });
            }

            var foundSeriesLesson = await _seriesLessonRepository.GetById(id);

            if (foundSeriesLesson is null)
            {
                return NotFound(new ServerResponse { Success = false, Message = "Series Lesson does not exist" });
            }

            await _seriesLessonRepository.Delete(foundSeriesLesson);

            return NoContent();
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SeriesLessonRepresent>>> GetSeriesLessons()
    {
        try
        {
            var seriesLessons = await _seriesLessonRepository.GetAll();

            var projections = seriesLessons.Select(sl => new SeriesLessonRepresent()
            {
                Id = sl.Id,
                Title = sl.Title,
                Cover = sl.Cover,
                Hex = sl.Hex,
                CategoryId = sl.CategoryId,
                CategoryTitle = sl.Category.Title,
                SeriesId = sl.SeriesId,
                SeriesTitle = sl.Series.Title,
                Content = sl.Content,
                CreatedAt = sl.CreatedAt,
                UpdatedAt = sl.UpdatedAt,
            });

            return Ok(new ServerResponse<IEnumerable<SeriesLessonRepresent>>() { Success = true, Data = projections });
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<SeriesLesson>> GetSeriesLessonsById(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Id is required" });
            }

            var seriesLesson = await _seriesLessonRepository.GetById(id);

            if (seriesLesson is null)
            {
                return NotFound(new ServerResponse { Success = false, Message = "Series Lesson does not exist" });
            }

            return Ok(new ServerResponse<SeriesLesson>() { Success = true, Data = seriesLesson });
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [AllowAnonymous]
    [HttpGet("series/{seriesId}")]
    public async Task<ActionResult<IEnumerable<SeriesLesson>>> GetSeriesLessonsBySeriesId(string seriesId)
    {
        try
        {
            if (string.IsNullOrEmpty(seriesId))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Series is required" });
            }

            var series = await _seriesRepository.GetById(seriesId);

            if (series is null)
            {
                return NotFound(new ServerResponse { Success = false, Message = "Series does not exist" });
            }

            var seriesLessons = await _seriesLessonRepository.GetBySeriesId(seriesId);

            return Ok(new ServerResponse<IEnumerable<SeriesLesson>>() { Success = true, Data = seriesLessons });
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [AllowAnonymous]
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<SeriesLesson>>> GetSeriesLessonsByCategoryId(string categoryId)
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

            var seriesLessons = await _seriesLessonRepository.GetByCategoryId(categoryId);

            return Ok(new ServerResponse<IEnumerable<SeriesLesson>>() { Success = true, Data = seriesLessons });
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSeriesLesson(string id, UpdateSeriesLessonDto dto)
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

            var foundSeriesLesson = await _seriesLessonRepository.GetById(id);

            if (foundSeriesLesson is null)
            {
                return NotFound(new ServerResponse() { Success = false, Message = "Series Lesson does not exist" });
            }

            SeriesLesson seriesLesson = new()
            {
                Id = foundSeriesLesson.Id,
                CategoryId = foundSeriesLesson.CategoryId,
                SeriesId = foundSeriesLesson.SeriesId,
                Title = dto.Title ?? foundSeriesLesson.Title,
                Cover = dto.Cover ?? foundSeriesLesson.Cover,
                Hex = dto.Hex ?? foundSeriesLesson.Hex,
                Content = dto.Content ?? foundSeriesLesson.Content,
                CreatedAt = foundSeriesLesson.CreatedAt,
                UpdatedAt = DateTime.Now,
            };

            await _seriesLessonRepository.Update(seriesLesson);

            return NoContent();
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }
}