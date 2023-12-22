using System.Web;
using Microsoft.EntityFrameworkCore;

namespace Project.Controllers;

public class SubscribeController : Controller
{

    private readonly AppDbContext _context;

    public SubscribeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(string email)
    {
        string previousUrl = Request.Headers["Referer"].ToString();

        var emails = await _context.Subscribes.ToListAsync();

        foreach (var item in emails)
        {

            if (item.Email.Trim().ToUpper() == email.Trim().ToUpper())
            {

                ModelState.AddModelError("email", "Email already exists.");
                return BadRequest("Email already exists.");

            }
        }


        Subscribe subscribe = new() { Email = email };


        await _context.Subscribes.AddAsync(subscribe);
        await _context.SaveChangesAsync();


        return Redirect(previousUrl);

    }
}
