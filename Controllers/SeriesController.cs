using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NerdwikiServer.Data.Base;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Dtos;
using NerdwikiServer.Repositories.Interfaces;

namespace NerdwikiServer.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SeriesController(ISeriesRepository seriesRepository, ICategoryRepository categoryRepository) : ControllerBase
{
    private readonly ISeriesRepository _seriesRepository = seriesRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    [HttpPost]
    public async Task<ActionResult<Series>> CreateSeries(CreateSeriesDto dto)
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

            var foundSeries = await _seriesRepository.GetById(dto.Id);

            if (foundSeries is not null)
            {
                return Conflict(new ServerResponse { Success = false, Message = "Series is already in the repository" });
            }

            Series series = new()
            {
                Id = dto.Id,
                CategoryId = dto.CategoryId,
                Title = dto.Title,
                Cover = dto.Cover,
                Hex = dto.Hex,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            await _seriesRepository.Create(series);

            return Created();
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSeries(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Id is required" });
            }

            var foundSeries = await _seriesRepository.GetById(id);

            if (foundSeries is null)
            {
                return NotFound(new ServerResponse { Success = false, Message = "Series does not exist" });
            }

            await _seriesRepository.Delete(foundSeries);

            return NoContent();
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Series>>> GetSeries()
    {
        try
        {
            var series = await _seriesRepository.GetAll();

            return Ok(new ServerResponse<IEnumerable<Series>>() { Success = true, Data = series });
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<Series>> GetSeriesById(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Id is required" });
            }

            var series = await _seriesRepository.GetById(id);

            if (series is null)
            {
                return NotFound(new ServerResponse { Success = false, Message = "Series does not exist" });
            }

            return Ok(new ServerResponse<Series>() { Success = true, Data = series });
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [AllowAnonymous]
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<Series>>> GetSeriesByCategoryId(string categoryId)
    {
        try
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Id is required" });
            }

            var category = await _categoryRepository.GetById(categoryId);

            if (category is null)
            {
                return NotFound(new ServerResponse { Success = false, Message = "Category does not exist" });
            }

            var series = await _seriesRepository.GetByCategoryId(categoryId);

            return Ok(new ServerResponse<IEnumerable<Series>>() { Success = true, Data = series });
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateSeries(string id, UpdateSeriesDto dto)
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

            var foundSeries = await _seriesRepository.GetById(id);

            if (foundSeries is null)
            {
                return NotFound(new ServerResponse() { Success = false, Message = "Series does not exist" });
            }

            Series series = new()
            {
                Id = foundSeries.Id,
                CategoryId = foundSeries.CategoryId,
                Title = dto.Title ?? foundSeries.Title,
                Cover = dto.Cover ?? foundSeries.Cover,
                Hex = dto.Hex ?? foundSeries.Hex,
                CreatedAt = foundSeries.CreatedAt,
                UpdatedAt = DateTime.Now,
            };

            await _seriesRepository.Update(series);

            return NoContent();
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }
}