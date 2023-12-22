using System.ComponentModel.DataAnnotations;
namespace Project.Areas.Admin.ViewModels;

public class TeacherViewModel
{
    public int TeacherId { get; set; }

    public IFormFile? Image { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Profession { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string Degree { get; set; }

    [Required]
    public string Experience { get; set; }

    [Required]
    public string Hobbies { get; set; }

    [Required]
    public string Faculty { get; set; }

    [Required , DataType(DataType.EmailAddress)]
    public string Mail { get; set; }

    [Required, DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; }
}
