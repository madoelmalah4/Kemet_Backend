using Kemet_api.DTOs.Category;
using Kemet_api.Models;
using Kemet_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kemet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class CategoryController : ControllerBase
    {
        private readonly IRepository<Category> _categoryRepository;

        public CategoryController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var dtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Title = c.Title
            });
            return Ok(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Title = dto.Title
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllCategories), new { id = category.Id }, new CategoryDto
            {
                Id = category.Id,
                Title = category.Title
            });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found" });

            await _categoryRepository.DeleteAsync(category);
            return NoContent();
        }
    }
}
