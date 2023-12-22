using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Context;
using Project.Models;
namespace Project.Controllers;

public class CourseController : Controller
{
    private readonly AppDbContext _context;

    public CourseController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var coursesQuery = await _context.Courses.Where(c => !c.IsDeleted).Include(c => c.CourseCategories).ThenInclude(c => c.Category).ToListAsync();

        return View(coursesQuery);
    }

    public async Task<IActionResult> Details(int id)
    {
        var courseQuery =await _context.Courses.Where(c => !c.IsDeleted && c.Id == id).Include(c => c.CourseCategories).ThenInclude(c => c.Category).FirstOrDefaultAsync();

        if (courseQuery == null)return NotFound();

        CourseCategoryViewModel courseCategoryViewModel = new() { 
            Courses = courseQuery,
            Categories = await _context.Categories.Include(c => c.CourseCategories).ThenInclude(c=>c.Course).Where(p => !p.IsDeleted).ToListAsync()
        };


        return View(courseCategoryViewModel);
    }

    public async Task<IActionResult> CoursesByCategory(int categoryId)
    {
        var category = await _context.Categories.Include(c => c.CourseCategories).ThenInclude(cc => cc.Course).FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null) return NotFound();

        var courses = category.CourseCategories.Where(c=> c.Course.IsDeleted==false).Select(cc => cc.Course).ToList();

        return View(courses);
    }
}
