using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Project.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Project.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin, Moderator")]
public class UsersController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var activeUser = HttpContext.User.Identity.Name; // Login olmuş useri tapmaq

        var users = await _userManager.Users.Where(u => u.UserName != activeUser).ToListAsync(); // Login olmuş useri table`da göstərməmək üçün

        List<AllUserViewModel> allUsers = new List<AllUserViewModel>();

        foreach (var user in users) // Bütün userləri foreach`ə salırıq. Hər bir useri lazım olan məlumatlarını Yaratdığımız ViewModel`ə set edirik 
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            allUsers.Add(new AllUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.UserName,
                Role = userRoles.FirstOrDefault(),
                IsActive = user.IsActive
            });

        }

        return View(allUsers);
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);

        AllUserViewModel allUserViewModel = new() {
            Fullname = user.Fullname,
            Email = user.Email,
            Username = user.UserName,
            Role = userRoles.FirstOrDefault()
        };



        return View(allUserViewModel);
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeRole(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);

        if (userRoles.FirstOrDefault() == "Admin")
        {
            return BadRequest();
        }

        UserViewModel userViewModel = new()
        {
            Role = userRoles.FirstOrDefault() // Many to many relation olduğuna görə List return edir. Amma bizim user`imizin bir role`u olduğu üçün FirstOrDefault`la götürürük
        };

        ViewBag.Roles = _roleManager.Roles.ToList(); // Bütün role`ları view`a ötürürük

        return View(userViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeRole(string id, UserViewModel userViewModel, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id); // Find user by id
        if (user == null)
            return NotFound();

        var userRole = await _userManager.GetRolesAsync(user); // Get Role

        if (userRole.FirstOrDefault() == "Admin")// Role`unu dəyişdirmək istədiyimiz admin`dirsə error verir
        {
            return BadRequest();
        }

        user.IsActive = isActive; // Update the isActive status


        await _userManager.RemoveFromRoleAsync(user, userRole.FirstOrDefault()); //Remove old role

        await _userManager.AddToRoleAsync(user, userViewModel.Role); // add new role

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleActiveStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        //var activeUser = HttpContext.User.Identity.IsAuthenticated;
        var userRole = await _userManager.GetRolesAsync(user); // Get Role

        if (userRole.FirstOrDefault() == "Admin")// Role`unu dəyişdirmək istədiyimiz admin`dirsə error verir
        {
            return BadRequest();
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        return RedirectToAction(nameof(Index));
    }

}
