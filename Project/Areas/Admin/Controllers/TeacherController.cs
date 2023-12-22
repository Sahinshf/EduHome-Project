using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using Project.Areas.Admin.ViewModels;
using Project.Utils;
using System.Xml.Linq;

namespace Project.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin, Moderator")]
public class TeacherController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public TeacherController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var teacher = await _context.Teachers.Where(c => !c.IsDeleted).ToListAsync();

        return View(teacher);
    }

    public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeacherViewModel teacherViewModel)
    {

        if (!ModelState.IsValid) return View();

        if (teacherViewModel.Image == null) { ModelState.AddModelError("Image", "Select Image"); return View(); }
        if (!teacherViewModel.Image.CheckFileType("image"))
        {
            ModelState.AddModelError("Image", "File Type Must be Image.");
            return View();
        }


        string filename = $"{Guid.NewGuid()}-{teacherViewModel.Image.FileName}";
        string path = Path.Combine(_webHostEnvironment.WebRootPath, "img", "teacher", filename);

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            await teacherViewModel.Image.CopyToAsync(stream);
        }

        Teacher teacher = new()
        {
            Name = teacherViewModel.Name,
            Image = filename,
            Profession = teacherViewModel.Profession,
            Description = teacherViewModel.Description,
            Degree = teacherViewModel.Degree,
            Faculty = teacherViewModel.Faculty,
            Hobbies = teacherViewModel.Hobbies,
            Experience = teacherViewModel.Experience,
            Mail = teacherViewModel.Mail,
            PhoneNumber = teacherViewModel.PhoneNumber,
            IsDeleted = false
        };

        await _context.Teachers.AddAsync(teacher);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int id)
    {
        Teacher teacher = await _context.Teachers.Where(c => !c.IsDeleted).Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == id);
        if (teacher == null) return View();

        TeacherViewModel teacherViewModel = new()
        {
            Name = teacher.Name,
            Profession = teacher.Profession,
            Description = teacher.Description,
            Degree = teacher.Degree,
            Faculty = teacher.Faculty,
            Hobbies = teacher.Hobbies,
            Experience = teacher.Experience,
            Mail = teacher.Mail,
            PhoneNumber = teacher.PhoneNumber,
        };

        return View(teacherViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, TeacherViewModel teacherViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        Teacher teacher = await _context.Teachers.Where(c => !c.IsDeleted).Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == id);
        if (teacher == null) return View();

        if (teacherViewModel.Image != null)
        {
            if (!teacherViewModel.Image.CheckFileType("image"))
            {
                ModelState.AddModelError("Image", "File Type Must be Image.");
                return View();
            }
            var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "teacher", teacher.Image);
            FileService.DeleteFile(oldPath);

            var filename = $"{Guid.NewGuid()} - {teacherViewModel.Image.FileName}";
            var newPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "teacher", filename);

            using (FileStream stream = new FileStream(newPath, FileMode.Create))
            {
                await teacherViewModel.Image.CopyToAsync(stream);
            }

            teacher.Image = filename;
        }
        teacher.Name = teacherViewModel.Name;
        teacher.Profession = teacherViewModel.Profession;
        teacher.Description = teacherViewModel.Description;
        teacher.Degree = teacherViewModel.Degree;
        teacher.Faculty = teacherViewModel.Faculty;
        teacher.Hobbies = teacherViewModel.Hobbies;
        teacher.Experience = teacherViewModel.Experience;
        teacher.Mail = teacherViewModel.Mail;
        teacher.PhoneNumber = teacherViewModel.PhoneNumber;


        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        Teacher teacher = await _context.Teachers.Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (teacher is null) return BadRequest();

        teacher.Skills = teacher.Skills.Where(c => !c.IsDeleted).ToList();

        return View(teacher);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteService(int id)
    {
        if (!ModelState.IsValid) return View();


        Teacher teacher = await _context.Teachers.Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (teacher == null) return NotFound();

        teacher.IsDeleted = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        Teacher teacher = await _context.Teachers.Include(c => c.Skills).Include(c => c.SocialMedias).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (teacher == null) return NotFound();

        teacher.Skills = teacher.Skills.Where(c => !c.IsDeleted).ToList();
        teacher.SocialMedias = teacher.SocialMedias.Where(c => !c.IsDeleted).ToList();

        return View(teacher);
    }
}
