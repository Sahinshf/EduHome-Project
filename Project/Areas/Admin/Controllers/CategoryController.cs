using Gherkin.CucumberMessages.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.ViewModels;
using Project.Context;
using Project.Models;

namespace Project.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin, Moderator")]
public class CategoryController : Controller
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories.Where( c=> !c.IsDeleted).ToListAsync();
        return View(categories);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryViewModel categoryViewModel)
    {
        if (!ModelState.IsValid) { return View(); }

        bool isExist = await _context.Categories.AnyAsync(c => c.Name == categoryViewModel.Name);
        if (isExist)
        {
            ModelState.AddModelError("Name", "This Category is already exist");
            return View();
        }

        Category category = new Category
        {

            Name = categoryViewModel.Name,
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();


        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        Category category = await _context.Categories?.Include(c => c.CourseCategories).ThenInclude(c => c.Course).FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

        if (category is null) return NotFound();

        category.CourseCategories = category.CourseCategories
        .Where(cc => cc.Course.IsDeleted == false)
        .ToList();

        return View(category);
    }

    public async Task<IActionResult> Update(int id)
    {
        Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);
        if (category is null) return NotFound();

        CategoryViewModel categoryViewModel = new()
        {
            Id = id,
            Name = category.Name
        };

        return View(categoryViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, CategoryViewModel categoryViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        if (categoryViewModel is null)
        {
            return NotFound();
        }

        Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        category.Name = categoryViewModel.Name;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (category is null) return NotFound();
        

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteService( int id )
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (category is null) return NotFound();

        category.IsDeleted = true;
        await _context.SaveChangesAsync();


        return RedirectToAction(nameof(Index));
    }

}
