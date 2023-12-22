using Microsoft.Build.Framework;

namespace Project.Areas.Admin.ViewModels;

public class EventViewModel
{
    public int EventId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public IFormFile? Image { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    public DateTime EventStartTime { get; set; }

    [Required]
    public DateTime EventEndTime { get; set; }

    [Required]
    public string Venue { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public int[] SpeakersIds { get; set; }
}
