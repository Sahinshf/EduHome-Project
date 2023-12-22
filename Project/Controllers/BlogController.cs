using Microsoft.EntityFrameworkCore;

namespace Project.Controllers;

public class BlogController : Controller
{
    private readonly AppDbContext _context;

    public BlogController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var blogs = await _context.Blogs.Where(c => !c.IsDeleted).ToListAsync();

        return View(blogs);
    }

    public async Task<IActionResult> Details(int id)
    {
        var blogs = await _context.Blogs.FirstOrDefaultAsync(c=>!c.IsDeleted && c.Id == id);
        if(blogs == null) return NotFound();

        return View(blogs);
    }
}
