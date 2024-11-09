using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NerdwikiServer.Data.Base;
using NerdwikiServer.Data.Entities;
using NerdwikiServer.Dtos;
using NerdwikiServer.Repositories.Interfaces;

namespace NerdwikiServer.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CategoriesController(ICategoryRepository categoryRepository) : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
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

            var foundCategory = await _categoryRepository.GetById(dto.Id);

            if (foundCategory is not null)
            {
                return Conflict(new ServerResponse { Success = false, Message = "Category is already in the repository" });
            }

            Category category = new()
            {
                Id = dto.Id,
                Title = dto.Title,
                Cover = dto.Cover,
                Hex = dto.Hex,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            await _categoryRepository.Create(category);

            return Created();
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Id is required" });
            }

            var foundCategory = await _categoryRepository.GetById(id);

            if (foundCategory is null)
            {
                return NotFound(new ServerResponse { Success = false, Message = "Category does not exist" });
            }

            await _categoryRepository.Delete(foundCategory);

            return NoContent();
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        try
        {
            var categories = await _categoryRepository.GetAll();

            return Ok(new ServerResponse<IEnumerable<Category>>() { Success = true, Data = categories });
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetCategoryById(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ServerResponse { Success = false, Message = "Id is required" });
            }

            var category = await _categoryRepository.GetById(id);

            if (category is null)
            {
                return NotFound(new ServerResponse { Success = false, Message = "Category does not exist" });
            }

            return Ok(new ServerResponse<Category>() { Success = true, Data = category });
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(string id, UpdateCategoryDto dto)
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

            var foundCategory = await _categoryRepository.GetById(id);

            if (foundCategory is null)
            {
                return NotFound(new ServerResponse() { Success = false, Message = "Category does not exist" });
            }

            Category category = new()
            {
                Id = foundCategory.Id,
                Title = dto.Title ?? foundCategory.Title,
                Cover = dto.Cover ?? foundCategory.Cover,
                Hex = dto.Hex ?? foundCategory.Hex,
                CreatedAt = foundCategory.CreatedAt,
                UpdatedAt = DateTime.Now,
            };

            await _categoryRepository.Update(category);

            return NoContent();
        }
        catch (Exception e)
        {
            return Problem(detail: e.Message, statusCode: 500);
        }
    }
}