namespace Project.Models;

public class Speaker
{
    public int Id { get; set; }
    public string? Image { get; set; }
    public string? Name { get; set; }
    public string? Profession { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<EventSpeaker>? EventSpeakers{ get; set; }
}
