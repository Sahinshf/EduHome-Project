using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin, Moderator")]
public class SocialMediaController : Controller
{
    private readonly AppDbContext _context;

    public SocialMediaController(AppDbContext context)
    {
        _context = context;

    }

    public async Task<IActionResult> Index()
    {
        var socialMedias = await _context.SocialMedias.Where(s => !s.IsDeleted).Include(c => c.Teacher).Where(z=>!z.Teacher.IsDeleted).ToListAsync();

        return View(socialMedias);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Teachers = await _context.Teachers.Where(t => !t.IsDeleted).ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] string socialMedia, SocialMediaViewModel socialMediaViewModel)
    {
        ViewBag.Teachers = await _context.Teachers.Where(t => !t.IsDeleted).ToListAsync();
        if (!ModelState.IsValid) return View();

        SocialMedia socialMediaAcc = new()
        {
            Name = socialMedia,
            TeacherId = socialMediaViewModel.TeacherId,
            Account = socialMediaViewModel.Account,
        };

        await _context.AddAsync(socialMediaAcc);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int id)
    {
        ViewBag.Teachers = await _context.Teachers.Where(t => !t.IsDeleted).ToListAsync();


        SocialMedia socialMedia =await _context.SocialMedias.Where(c=>!c.IsDeleted).Include(c=>c.Teacher).FirstOrDefaultAsync(c=> c.Id == id);
        if (socialMedia == null) return BadRequest();

        SocialMediaViewModel socialMediaViewModel = new() { 
            Id= id,
            Account= socialMedia.Account,
            TeacherId= socialMedia.TeacherId
        };

        ViewBag.SelectedSocialMedia = socialMedia.Name;
        ViewBag.SelectedTeacher = socialMedia.Teacher.Name;


        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, SocialMediaViewModel socialMediaViewModel)
    {
        if (!ModelState.IsValid) return View();

        SocialMedia socialMedia =await _context.SocialMedias.Where(c=>!c.IsDeleted).FirstOrDefaultAsync(c=>c.Id == id);
        if (socialMedia == null) return NotFound();


        socialMedia.Account = socialMediaViewModel.Account;
        socialMedia.TeacherId = socialMediaViewModel.TeacherId;
         
        await _context.SaveChangesAsync();

        
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        SocialMedia socialMedia = await _context.SocialMedias.Where(c => !c.IsDeleted).Include(c=>c.Teacher).FirstOrDefaultAsync(c => c.Id == id);
        if (socialMedia == null) return NotFound();


        return View(socialMedia);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteServie(int id)
    {
        if(!ModelState.IsValid) return View();

        SocialMedia socialMedia = await _context.SocialMedias.Where(c => !c.IsDeleted).FirstOrDefaultAsync(c => c.Id == id);
        if (socialMedia == null) return NotFound();

        socialMedia.IsDeleted = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        SocialMedia socialMedia = await _context.SocialMedias.Where(c => !c.IsDeleted).Include(c => c.Teacher).FirstOrDefaultAsync(c => c.Id == id);

        if (socialMedia == null) return NotFound();


        return View(socialMedia);
    }
}
