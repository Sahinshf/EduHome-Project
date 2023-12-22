using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels;

public class SubscribeViewModel
{
    public int Id { get; set; }
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
}
