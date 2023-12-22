using System.ComponentModel.DataAnnotations;

namespace Project.ViewModel;


public class RegisterViewModel
{
    [Required, MaxLength(100)]
    public string? Fullname { get; set; }

    [Required, MaxLength(100)]
    public string? Username { get; set; }
    
    [Required, MaxLength(256), DataType(DataType.EmailAddress)] //DataType`ı string`ə görə text olacaq ona Görə datatype `ı DataType enum`ından istifadə edərək göstəririk 
    public string? Email { get; set; }

    [Required, DataType(DataType.Password)]
    public string? Password { get; set; }
    
    [Required, DataType(DataType.Password), Compare(nameof(Password))] // ConfirmPassword`un Password`la Eyni olub olmadığını yoxlamaq üçün Compare`dən istifadə edirik
    public string? ConfirmPassword { get; set; }
}
