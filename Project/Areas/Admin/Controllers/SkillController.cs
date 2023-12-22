using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Project.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin, Moderator")]
public class SkillController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SkillController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index(int? id)
    {
        ViewBag.Teachers = await _context.Teachers.Where(t => !t.IsDeleted).ToListAsync();

        if (id == null)
        {
            var skills = await _context.Skills.Where(c => !c.IsDeleted).Include(c => c.Teacher).Where(c=> !c.Teacher.IsDeleted).ToListAsync();
            return View(skills);

        }
        else
        {
            var skills =  _context.Skills.Where(c => !c.IsDeleted).Include(c => c.Teacher);
            var  teacherSkills = await skills.Where(c=>c.Teacher.Id== id).ToListAsync();

            return View(teacherSkills);
        }
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Teachers = await _context.Teachers.Where(t => !t.IsDeleted).ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SkillViewModel skillViewModel)
    {
        if (!ModelState.IsValid) return View();

        Skill skill = new()
        {
            TeacherId = skillViewModel.TeacherId,
            Name = skillViewModel.Name,
            Percentage = skillViewModel.Percentage,
        };

        await _context.Skills.AddAsync(skill);
        await _context.SaveChangesAsync();



        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int id)
    {
        ViewBag.Teachers = await _context.Teachers.Where(t => !t.IsDeleted).ToListAsync();


        Skill skill = _context.Skills.Where(skill => !skill.IsDeleted).Include(c => c.Teacher).FirstOrDefault(c => c.Id == id);
        if (skill is null) return BadRequest();

        SkillViewModel skillViewModel = new()
        {
            Name = skill.Name,
            Percentage = skill.Percentage,
        };

        ViewBag.SelectedTeacher = skill.Teacher.Name;

        return View(skillViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, SkillViewModel skillViewModel)
    {
        if (!ModelState.IsValid) return View();

        Skill skill = await _context.Skills.Where(c => !c.IsDeleted).Include(s => s.Teacher).FirstOrDefaultAsync(s => s.Id == id);
        if (skill is null) return BadRequest();

        skill.Name = skillViewModel.Name;
        skill.Percentage = skillViewModel.Percentage;
        skill.TeacherId = skillViewModel.TeacherId;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        Skill skill = await _context.Skills.Where(c => !c.IsDeleted).Include(s => s.Teacher).FirstOrDefaultAsync(s => s.Id == id);
        if (skill == null) return NotFound();


        return View(skill);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteService(int id)
    {
        if (!ModelState.IsValid) return View();

        Skill skill = await _context.Skills.Where(c => !c.IsDeleted).Include(s => s.Teacher).FirstOrDefaultAsync(s => s.Id == id);
        if (skill == null) return NotFound();

        skill.IsDeleted = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));

    }

    public async Task<IActionResult> Details(int id)
    {
        Skill skill = await _context.Skills.Where(c => !c.IsDeleted).Include(s => s.Teacher).FirstOrDefaultAsync(s => s.Id == id);
        if (skill == null) return NotFound();


        return View(skill);
    }

}
