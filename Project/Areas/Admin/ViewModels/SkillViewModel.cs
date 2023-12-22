using Microsoft.Build.Framework;
namespace Project.Areas.Admin.ViewModels;

public class SkillViewModel
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public int Percentage { get; set; }
    [Required]
    public int TeacherId { get; set; }
}
