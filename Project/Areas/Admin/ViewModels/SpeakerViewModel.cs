using System.ComponentModel.DataAnnotations;
namespace Project.Areas.Admin.ViewModels;

public class SpeakerViewModel
{
    public int Id { get; set; }
    public IFormFile? Image { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Profession { get; set; }
}
