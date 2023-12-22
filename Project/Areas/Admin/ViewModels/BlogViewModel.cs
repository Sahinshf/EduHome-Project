using Microsoft.Build.Framework;

namespace Project.Areas.Admin.ViewModels;

public class BlogViewModel
{

    public int Id { get; set; }

    public IFormFile? Image { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public int Comment { get; set; }

    [Required]
    public DateTime Created { get; set; }
}
