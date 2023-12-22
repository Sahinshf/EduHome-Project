using Gherkin.CucumberMessages.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.ViewModels;
using Project.Models;
using Project.Utils;
using System.Security.Policy;
using TechTalk.SpecFlow;

namespace Project.Areas.Admin.Controllers;

[Area("Admin")]
//[Authorize(Roles = "Admin, Moderator")]
public class EventController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public EventController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var events = await _context.Events.Include(c => c.EventSpeakers).ThenInclude(e => e.Speaker).Where(c => !c.IsDeleted).ToArrayAsync();

        var allEvents = new List<AllEventsViewModel>();

        foreach (var eventt in events)
        {
            var speaker = eventt.EventSpeakers.Where(c => !c.Speaker.IsDeleted).Select(c => c.Speaker.Name);

            var allEventViewModel = new AllEventsViewModel
            {
                Id = eventt.Id,
                Name = eventt.Name,
                Image = eventt.Image,
                Speakers = string.Join(", ", speaker)
            };

            allEvents.Add(allEventViewModel);

        }

        return View(allEvents);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Speakers = await _context.Speakers.ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventViewModel eventViewModel)
    {
        ViewBag.Speakers = await _context.Speakers.ToListAsync();

        if (!ModelState.IsValid) return View();
        if (eventViewModel.Image is null)
        {
            ModelState.AddModelError("Image", "Add Image");
            return View();
        }
        if (!eventViewModel.Image.CheckFileType("image"))
        {
            ModelState.AddModelError("Image", "File Type Must be Image.");
            return View();
        }
        if (eventViewModel.SpeakersIds.Length == 0)
        {
            ModelState.AddModelError("SpeakersIds", "Select Category");
            return View();
        }

        string filename = $"{Guid.NewGuid()}-{eventViewModel.Image.FileName}";
        string path = Path.Combine(_webHostEnvironment.WebRootPath, "img", "event", filename);

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            await eventViewModel.Image.CopyToAsync(stream);
        }

        Event newEvent = new()
        {
            Name = eventViewModel.Name,
            Image = filename,
            EventDate = eventViewModel.EventDate,
            EventStartTime = eventViewModel.EventStartTime,
            EventEndTime = eventViewModel.EventEndTime,
            Venue = eventViewModel.Venue,
            Description = eventViewModel.Description,
            IsDeleted = false
        };

        List<EventSpeaker> speakers = new List<EventSpeaker>();

        foreach (var speaker in eventViewModel.SpeakersIds)
        {
            var allSpeaker = new EventSpeaker
            {
                SpeakerId = speaker,
                EventId = 9
            };
            speakers.Add(allSpeaker);
        }

        newEvent.EventSpeakers = speakers;
        await _context.Events.AddAsync(newEvent);

        EmailHelper emailHelper = new EmailHelper();


        foreach (var user in _context.Subscribes)
        {

            MailRequestViewModel mailRequestViewModel = new()
            {

                ToEmail = user.Email,
                Subject = "New Event",
                Body = $"<p>Name : {eventViewModel.Name} | Time : {eventViewModel.EventDate}</p>"
            };
            await emailHelper.SendEmailAsync(mailRequestViewModel);
        }



        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int id)
    {
        ViewBag.Speakers = await _context.Speakers.ToListAsync();

        Event eventt = await _context.Events.Include(e => e.EventSpeakers).ThenInclude(e => e.Speaker).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (eventt == null) return NotFound();

        ViewBag.SelectedSpeakers = string.Join(", ", eventt.EventSpeakers.Where(cc => !cc.Speaker.IsDeleted).Select(c => c.Speaker.Name));

        EventViewModel eventViewModel = new EventViewModel
        {
            Name = eventt.Name,
            EventDate = eventt.EventDate,
            EventStartTime = eventt.EventStartTime,
            EventEndTime = eventt.EventEndTime,
            Venue = eventt.Venue,
            Description = eventt.Description,
        };

        return View(eventViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, EventViewModel eventViewModel)
    {
        ViewBag.Speakers = await _context.Speakers.Where(c => !c.IsDeleted).ToListAsync();

        if (!ModelState.IsValid) return View();
        if (eventViewModel.SpeakersIds.Length == 0)
        {
            ModelState.AddModelError("SpeakersIds", "Select Category");
            return View();
        }

        Event eventt = await _context.Events.Include(c => c.EventSpeakers).ThenInclude(c => c.Speaker).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (eventt == null) return NotFound();

        if (eventViewModel.Image != null)
        {
        if (!eventViewModel.Image.CheckFileType("image"))
        {
            ModelState.AddModelError("Image", "File Type Must be Image.");
            return View();
        }
            var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "event", eventt.Image);
            FileService.DeleteFile(oldPath);

            var filename = $"{Guid.NewGuid()} - {eventViewModel.Image.FileName}";
            var newPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "event", filename);

            using (FileStream stream = new FileStream(newPath, FileMode.Create))
            {
                await eventViewModel.Image.CopyToAsync(stream);
            }

            eventt.Image = filename;
        }

        eventt.Name = eventViewModel.Name;
        eventt.Venue = eventViewModel.Venue;
        eventt.Description = eventViewModel.Description;
        eventt.EventDate = eventViewModel.EventDate;
        eventt.EventStartTime = eventViewModel.EventStartTime;
        eventt.EventStartTime = eventViewModel.EventStartTime;

        if (eventt.EventSpeakers != null)
            _context.EventsSpeaker.RemoveRange(eventt.EventSpeakers);


        List<EventSpeaker> allSpeakers = new List<EventSpeaker>();

        foreach (var eventId in eventViewModel.SpeakersIds)
        {
            EventSpeaker eventSpeaker = new EventSpeaker
            {
                SpeakerId = eventId
            };
            allSpeakers.Add(eventSpeaker);
        }

        eventt.EventSpeakers = allSpeakers;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        Event eventt = await _context.Events.Include(c => c.EventSpeakers).ThenInclude(c => c.Speaker).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (eventt == null) return NotFound();

        eventt.EventSpeakers = eventt.EventSpeakers.Where(c => !c.Speaker.IsDeleted).ToList();


        return View(eventt);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteService(int id)
    {
        if (!ModelState.IsValid) return View();


        Event eventt = await _context.Events.Include(c => c.EventSpeakers).ThenInclude(c => c.Speaker).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (eventt == null) return NotFound();

        eventt.IsDeleted = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        Event eventt = await _context.Events.Include(c => c.EventSpeakers).ThenInclude(c => c.Speaker).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (eventt == null) return NotFound();

        eventt.EventSpeakers = eventt.EventSpeakers.Where(c => !c.Speaker.IsDeleted).ToList();


        return View(eventt);
    }
}
