using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Admin.ViewModels;

public class CategoryViewModel
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
}
