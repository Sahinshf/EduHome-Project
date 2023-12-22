using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Admin.ViewModels;

public class SocialMediaViewModel
{
    public int Id { get; set; }
    [Required]
    public string Account { get; set; }
    [Required]
    public int TeacherId { get; set; }
}
