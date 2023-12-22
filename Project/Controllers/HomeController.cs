using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Project.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses.Where(c => !c.IsDeleted).OrderByDescending(o => o.ModifiedAt).Take(3).ToListAsync();
        var blogs = await _context.Blogs.Where(c => !c.IsDeleted).OrderByDescending(o => o.ModifiedAt).Take(3).ToListAsync();
        var events = await _context.Events.Where(c => !c.IsDeleted).OrderByDescending(o => o.ModifiedAt).Take(4).ToListAsync();


        HomeViewModel homeViewModel = new() { 
            Courses= courses,
            Events= events,
            Blogs= blogs
        };


        return View(homeViewModel);
    }
}
