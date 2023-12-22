using Gherkin.CucumberMessages.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.ViewModels;
using Project.Models;
using Project.Utils;
using System.Net.Mime;

namespace Project.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin, Moderator")]
public class SpeakerController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SpeakerController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var speaker = await _context.Speakers.Where(c => !c.IsDeleted).ToListAsync();

        return View(speaker);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SpeakerViewModel speakerViewModel)
    {
        if (!ModelState.IsValid) return View();
        if (speakerViewModel.Image == null)
        {
            ModelState.AddModelError("Image", "Select Image");
            return View();
        }
        if (!speakerViewModel.Image.CheckFileType("image"))
        {
            ModelState.AddModelError("Image", "File Type Must be Image.");
            return View();
        }

        string filename = $"{Guid.NewGuid()} - {speakerViewModel.Image.FileName}";
        string path = Path.Combine(_webHostEnvironment.WebRootPath, "img", "event", filename);

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            await speakerViewModel.Image.CopyToAsync(stream);
        }

        Speaker speaker = new()
        {
            Image = filename,
            Name = speakerViewModel.Name,
            Profession = speakerViewModel.Profession,
        };

        await _context.Speakers.AddAsync(speaker);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int id)
    {
        Speaker speaker = _context.Speakers.FirstOrDefault(speaker => speaker.Id == id);
        if (speaker.IsDeleted == true) return BadRequest();
        if (speaker is null) return NotFound();

        SpeakerViewModel speakerView = new()
        {
            Id = id,
            Name = speaker.Name,
            Profession = speaker.Profession
        };

        return View(speakerView);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(SpeakerViewModel speakerViewModel)
    {
        if (!ModelState.IsValid) return View();
        Speaker speaker = await _context.Speakers.Include(c => c.EventSpeakers).ThenInclude(c => c.Event).FirstOrDefaultAsync(c => c.Id == speakerViewModel.Id);

        if (speaker is null) return NotFound();

        if (speakerViewModel.Image != null)
        {
            if (!speakerViewModel.Image.CheckFileType("image"))
            {
                ModelState.AddModelError("Image", "File Type Must be Image.");
                return View();
            }
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "img", "event", speaker?.Image);
            FileService.DeleteFile(path);

            string filename = $"{Guid.NewGuid()}-{speakerViewModel.Image.FileName}";
            string newPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "event", filename);

            using (FileStream stream = new FileStream(newPath, FileMode.Create))
            {
                await speakerViewModel.Image.CopyToAsync(stream);
            }

            speaker.Image = filename;
        }

        speaker.Profession = speakerViewModel.Profession;
        speaker.Name = speakerViewModel.Name;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        Speaker speaker = await _context.Speakers.FirstOrDefaultAsync(c => c.Id == id);
        if (speaker is null) return NotFound();

        return View(speaker);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteService(int id)
    {
        Speaker speaker = await _context.Speakers.FirstOrDefaultAsync(c => c.Id == id);
        if (speaker is null) return NotFound();


        speaker.IsDeleted = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        Speaker speaker = await _context.Speakers.Include(c => c.EventSpeakers).ThenInclude(c => c.Event).FirstOrDefaultAsync(c => c.Id == id);
        if (speaker is null) return NotFound();

        speaker.EventSpeakers = speaker.EventSpeakers.Where(c => !c.Event.IsDeleted).ToList();

        return View(speaker);
    }
}
