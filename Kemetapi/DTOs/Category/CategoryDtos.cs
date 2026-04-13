using System;
using System.ComponentModel.DataAnnotations;

namespace Kemet_api.DTOs.Category
{
    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
    }

    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
