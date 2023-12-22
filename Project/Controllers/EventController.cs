using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Project.Controllers;

public class EventController : Controller
{
    private readonly AppDbContext _context;

    public EventController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var events =await _context.Events.Where(c => !c.IsDeleted).Include(c=>c.EventSpeakers).ThenInclude(c=>c.Speaker).ToListAsync();

        return View(events);
    }

    public async Task<IActionResult> Details(int id)
    {
        Event eventt =await _context.Events.Include(c=>c.EventSpeakers).ThenInclude(c=>c.Speaker).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        //var speakers = _context.Speakers.Where(s => !s.IsDeleted && s.Id == eventt.EventSpeakers.i).Include(c => c.EventSpeakers).ThenInclude(c => c.Event);
        //eventt.EventSpeakers.Where(c => !c.Speaker.IsDeleted).ToList();

        EventSpeakerViewModel eventSpeakerViewModel = new() {

            Event = eventt,
            Speakers = eventt.EventSpeakers.Where(c=>!c.Speaker.IsDeleted).ToList(),
        };


        return View(eventSpeakerViewModel);
    }
}
