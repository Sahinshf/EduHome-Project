using Microsoft.AspNetCore.Identity;

namespace Project.ViewComponents;

public class HeaderViewComponent : ViewComponent
{
    private readonly UserManager<AppUser> _userManager;

    public HeaderViewComponent(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        bool logged = HttpContext.User.Identity.IsAuthenticated;

        if(logged == false)
        {
            ViewBag.User = null;
        }
        else
        {
            var userName = HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);

            LoggedUserViewModel userViewModel = new() { 
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email
            };

            ViewBag.User = userViewModel;
        }

        return View(logged);
    }
}
