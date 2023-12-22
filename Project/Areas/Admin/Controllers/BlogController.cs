using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project.Utils;
using Project.Areas.Admin.ViewModels;

namespace Project.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin, Moderator")]
public class BlogController : Controller
{

    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BlogController(AppDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var blogs = await _context.Blogs.Where(c => !c.IsDeleted).ToListAsync();

        return View(blogs);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogViewModel blogViewModel)
    {
        if (!ModelState.IsValid) return View();


        if (blogViewModel.Image is null)
        {
            ModelState.AddModelError("Image", "Add Image");
            return View();
        }
        if (!blogViewModel.Image.CheckFileType("Image"))
        {
            ModelState.AddModelError("Image", "File Type Must be Image.");
            return View();
        }

        AppUser user = await _userManager.GetUserAsync(User);
        string fullName = user.Fullname;

        string filename = $"{Guid.NewGuid()}-{blogViewModel.Image.FileName}";
        string path = Path.Combine(_webHostEnvironment.WebRootPath, "img", "blog", filename);

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            await blogViewModel.Image.CopyToAsync(stream);
        }

        Blog blog = new()
        {
            Author = fullName,
            Image = filename,
            Name = blogViewModel.Name,
            Created = blogViewModel.Created,
            Comment = blogViewModel.Comment,
            Description = blogViewModel.Description,
        };

        await _context.Blogs.AddAsync(blog);
        await _context.SaveChangesAsync();


        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int id)
    {
        Blog blog = await _context.Blogs.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (blog == null) return NotFound();

        BlogViewModel blogViewModel = new()
        {
            Id = id,
            Name = blog.Name,
            Created = blog.Created,
            Comment = blog.Comment,
            Description = blog.Description,
        };

        return View(blogViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(BlogViewModel blogViewModel, int id)
    {
        if (!ModelState.IsValid) return View();

        Blog blog = await _context.Blogs.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (blog == null) return NotFound();

        if (blogViewModel.Image != null)
        {
            if (!blogViewModel.Image.CheckFileType("image"))
            {
                ModelState.AddModelError("Image", "File Type Must be Image.");
                return View();
            }
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "img", "blog", blog.Image);
            FileService.DeleteFile(path);

            string filename = $"{Guid.NewGuid()}-{blogViewModel.Image.FileName}";
            string newPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "blog", filename);

            using (FileStream stream = new FileStream(newPath, FileMode.Create))
            {
                await blogViewModel.Image.CopyToAsync(stream);
            }

            blog.Image = filename;
        }

        AppUser user = await _userManager.GetUserAsync(User);
        string fullName = user.Fullname;

        blog.Created = blogViewModel.Created;
        blog.Comment = blogViewModel.Comment;
        blog.Description = blogViewModel.Description;
        blog.Author = fullName;
        blog.Name = blogViewModel.Name;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        Blog blog = await _context.Blogs.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (blog == null) return NotFound();

        return View(blog);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Delete))]
    [Authorize(Roles = "Admin, Moderator")]
    public async Task<IActionResult> DeleteService(int id)
    {
        Blog blog = await _context.Blogs.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (blog == null) return NotFound();

        blog.IsDeleted = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        Blog blog = await _context.Blogs.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (blog == null) return NotFound();

        return View(blog);
    }
}
