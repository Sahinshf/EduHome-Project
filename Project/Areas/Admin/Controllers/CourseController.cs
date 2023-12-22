    using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.ViewModels;
using Project.Context;
using Project.Models;
using Project.Utils;
using TechTalk.SpecFlow.Assist;

namespace Project.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin, Moderator")]

public class CourseController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CourseController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses.Where(c => !c.IsDeleted).Include(c => c.CourseCategories).ThenInclude(c => c.Category).ToListAsync();

        var allCourse = new List<AllCourseViewModel>();

        foreach (var course in courses)
        {
            var categories = course.CourseCategories
            .Where(cc => cc.Category.IsDeleted == false)
            .Select(c => c.Category.Name);

            var courseViewModel = new AllCourseViewModel
            {
                Id = course.Id,
                Image = course.Image,
                Name = course.Name,
                Categories = string.Join(", ", categories)
            };

            allCourse.Add(courseViewModel);
        }

        return View(allCourse);
    }

    public IActionResult Create()
    {
        ViewBag.Categories = _context.Categories.AsEnumerable();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseViewModel courseViewModel)
    {
        ViewBag.Categories = _context.Categories.Where(c => !c.IsDeleted).AsEnumerable();


        if (!ModelState.IsValid)
        {
            return View();
        }
        if (courseViewModel.Image is null)
        {
            ModelState.AddModelError("Image", "Add Image");
            return View();
        }
        if (!courseViewModel.Image.CheckFileType("image"))
        {
            ModelState.AddModelError("Image", "File Type Must be Image.");
            return View();
        }

        if (courseViewModel.CategoriesIds.Length == 0)
        {
            ModelState.AddModelError("CategoriesIds", "Select Category");
            return View();
        }

        string filename = $"{Guid.NewGuid()}-{courseViewModel.Image.FileName}";
        string path = Path.Combine(_webHostEnvironment.WebRootPath, "img", "course", filename);

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            await courseViewModel.Image.CopyToAsync(stream);
        }

        Course course = new()
        {

            Name = courseViewModel.Name,
            Image = filename,
            Description = courseViewModel.Description,
            Start = courseViewModel.Start,
            ClassDuration = courseViewModel.ClassDuration,
            Duration = courseViewModel.Duration,
            SkillLevel = courseViewModel.SkillLevel,
            Language = courseViewModel.Language,
            StudentCount = courseViewModel.StudentCount,
            Price = courseViewModel.Price,
            IsDeleted = false
        };

        List<CourseCategory> allCategories = new List<CourseCategory>();

        foreach (var categoryId in courseViewModel.CategoriesIds)
        {
            CourseCategory courseCategory = new CourseCategory
            {
                CourseId = courseViewModel.Id,
                CategoryId = categoryId
            };
            allCategories.Add(courseCategory);
        }

        course.CourseCategories = allCategories;

        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();


        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int id)
    {
        ViewBag.Categories = _context.Categories.Where(c => !c.IsDeleted).AsEnumerable();

        Course? course = await _context.Courses.Include(c => c.CourseCategories).ThenInclude(c => c.Category).FirstOrDefaultAsync(c => c.Id == id);
        if (course.IsDeleted == true) return BadRequest();
        if (course is null) return NotFound();

        ViewBag.SelectedCategories = string.Join(", ", course.CourseCategories.Where(cc => cc.Category.IsDeleted == false).Select(c => c.Category.Name));

        CourseViewModel courseViewModel = new()
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Start = course.Start,
            Duration = course.Duration,
            ClassDuration = course.ClassDuration,
            SkillLevel = course.SkillLevel,
            Language = course.Language,
            StudentCount = course.StudentCount,
            Price = course.Price,
        };

        return View(courseViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, CourseViewModel courseViewModel)
    {
        ViewBag.Categories = _context.Categories.Where(c => !c.IsDeleted).AsEnumerable();

        if (!ModelState.IsValid) return View();

        if (courseViewModel.CategoriesIds.Length == 0)
        {
            ModelState.AddModelError("CategoriesIds", "Select Category");
            return View();
        }

        Course? course = await _context.Courses.Include(c => c.CourseCategories).ThenInclude(c => c.Category).FirstOrDefaultAsync(c => c.Id == id);

        if (course.IsDeleted == true) return BadRequest();
        if (course is null) return NotFound();

        if (courseViewModel.Image != null)
        {
            if (!courseViewModel.Image.CheckFileType("image"))
            {
                ModelState.AddModelError("Image", "File Type Must be Image.");
                return View();
            }
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "img", "course", course?.Image);
            FileService.DeleteFile(path);

            string filename = $"{Guid.NewGuid()}-{courseViewModel.Image.FileName}";
            string newPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "course", filename);

            using (FileStream stream = new FileStream(newPath, FileMode.Create))
            {
                await courseViewModel.Image.CopyToAsync(stream);
            }

            course.Image = filename;
        }

        course.Name = courseViewModel.Name;
        course.Description = courseViewModel.Description;
        course.Start = courseViewModel.Start;
        course.Duration = courseViewModel.Duration;
        course.ClassDuration = courseViewModel.ClassDuration;
        course.SkillLevel = courseViewModel.SkillLevel;
        course.Language = courseViewModel.Language;
        course.StudentCount = courseViewModel.StudentCount;
        course.Price = courseViewModel.Price;


        if (course.CourseCategories != null)
            _context.CourseCategory.RemoveRange(course.CourseCategories);


        List<CourseCategory> allCategories = new List<CourseCategory>();

        foreach (var categoryId in courseViewModel.CategoriesIds)
        {
            CourseCategory courseCategory = new CourseCategory
            {
                CourseId = courseViewModel.Id,
                CategoryId = categoryId
            };
            allCategories.Add(courseCategory);
        }

        course.CourseCategories = allCategories;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        Course? course = await _context.Courses.Include(c => c.CourseCategories).ThenInclude(c => c.Category).FirstOrDefaultAsync(c => c.Id == id);

        if (course.IsDeleted == true) return BadRequest();
        if (course is null) return NotFound();

        ViewBag.Categories = _context.Categories.Where(c => !c.IsDeleted).AsEnumerable();
        ViewBag.SelectedCategories = string.Join(", ", course.CourseCategories.Where(cc => cc.Category.IsDeleted == false).Select(c => c.Category.Name));

        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Delete))]
    [Authorize(Roles = "Admin, Moderator")]
    public async Task<IActionResult> DeleteService(int id)
    {
        Course? course = await _context.Courses.Include(c => c.CourseCategories).ThenInclude(c => c.Category).FirstOrDefaultAsync(c => c.Id == id);

        if (course.IsDeleted == true) return BadRequest();
        if (course is null) return NotFound();

        course.IsDeleted = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        Course? course = await _context.Courses.Include(c => c.CourseCategories).ThenInclude(c => c.Category).FirstOrDefaultAsync(c => c.Id == id);
        if (course is null) return NotFound();

        course.CourseCategories = course.CourseCategories.Where(c => c.Category.IsDeleted == false).ToList();

        ViewBag.Categories = string.Join(" , ", course.CourseCategories.Select(c => c.Category.Name));

        return View(course);
    }
}
