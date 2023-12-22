using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Admin.ViewModels;

public class CourseViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(256)]
    public string? Name { get; set; }

    [Required, MaxLength(2000)]
    public string? Description { get; set; }

    public IFormFile? Image { get; set; }

    [Required]
    public DateTime Start { get; set; }

    [Required]
    public string? Duration { get; set; }

    [Required]
    public string? ClassDuration { get; set; }

    [Required]
    public string? SkillLevel { get; set; }

    [Required]
    public string? Language { get; set; }

    [Required]
    public string? StudentCount { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int[] CategoriesIds { get; set; }

    public CourseViewModel()
    {
        CategoriesIds = new int[0];
    }
}
