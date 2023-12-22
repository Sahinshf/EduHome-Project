using Project.Models.Common;
using System.Drawing;

namespace Project.Models;

public class Event : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public DateTime EventDate { get; set; }
    public DateTime EventStartTime { get; set; }
    public DateTime EventEndTime { get; set; }
    public string Venue { get; set; }
    public string Description { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<EventSpeaker>? EventSpeakers { get; set; }
}
