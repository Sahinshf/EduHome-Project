using Microsoft.AspNetCore.Identity;
using Project.Utils;
using Project.ViewModel;

//using Microsoft.AspNetCore.Mvc.Formatters;
//using Microsoft.Build.Framework;
//using Microsoft.Build.ObjectModelRemoting;
//using NuGet.Protocol;

namespace Project.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager; // CRUD əməliyyatları üçün istifadə olunur
    private readonly SignInManager<AppUser> _signInManager; // Login logout əməliiytları üçün istifadə olunur
    private readonly RoleManager<IdentityRole> _roleManager; // Role əməliyyatları üçün istifadə olunur

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    public IActionResult Register()
    {
        if (User.Identity.IsAuthenticated)
        {
            return BadRequest(); // Login olub olmadığını yoxlayır
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
    {
        if (User.Identity.IsAuthenticated)
        {
            return BadRequest(); // Login olub olmadığını yoxlayır
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        AppUser newUser = new()
        {
            Fullname = registerViewModel.Fullname,
            UserName = registerViewModel.Username,
            Email = registerViewModel.Email,
            IsActive = true
        };

        var identityResult = await _userManager.CreateAsync(newUser, registerViewModel.Password); // user create olunur Passwordu method hash`ləyir

        if (!identityResult.Succeeded) // identityResult.Succeeded  user create olunub ya da olunmayıb onu retur edir
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError("", error.Description);

            }
            return View();
        }

        await _userManager.AddToRoleAsync(newUser, RoleType.Member.ToString()); // Register olan user ilk halda Member olur


        var token = Guid.NewGuid().ToString();  // Reset Token for user

        // Action       //Controller     //url`nin daxilində olanlar        // https
        string url = Url.Action("Verify", "Account", new { email = registerViewModel.Email, token = token }, HttpContext.Request.Scheme); // Verify Account üçün url yaradırıq

        // Send email section
        EmailHelper emailHelper = new EmailHelper();

        MailRequestViewModel mailRequestViewModel = new()
        {

            ToEmail = registerViewModel.Email,
            Subject = "Confirm Your Email",
            Body = $"<a href='{url}'>Confirm Your Email </a>"
        };

        await emailHelper.SendEmailAsync(mailRequestViewModel);

        return RedirectToAction(nameof(Login));
    }


    public async Task<IActionResult> Verify(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return BadRequest();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return NotFound();

        user.EmailConfirmed = true;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Login));

        }
        else
        {
            return View("VerificationError");
        }

    }

    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return BadRequest(); // Login olub olmadığını yoxlayır
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {
        if (User.Identity.IsAuthenticated) // Login olub olmadığını yoxlayır
            return BadRequest();


        if (!ModelState.IsValid) return View();

        var user = await _userManager.FindByNameAsync(loginViewModel.UserName); //Find user by user name
        if (user == null)
        {
            ModelState.AddModelError("", "Password or Username invalid ");
            return View();
        }
        if (user.IsActive == false)
        {
            ModelState.AddModelError("", "Your account is blocked ");
            return View();
        }
        if (user.EmailConfirmed == false)
        {
            ModelState.AddModelError("", "Please Confirm Your account ");
            return View();
        }

        //var signInResult = await _signInManager.PasswordSignInAsync(loginViewModel.UserName, loginViewModel.Password, loginViewModel.RememberMe, true)
        var signInResult = await _signInManager.PasswordSignInAsync(loginViewModel.UserName, loginViewModel.Password, loginViewModel.RememberMe, false); //useri login etmək

        if (signInResult.IsLockedOut)
        {
            ModelState.AddModelError("", "Your account is blocked temporary");
        }
        if (!signInResult.Succeeded)
        {
            ModelState.AddModelError("", "Password or Username invalid ");
            return View();
        }


        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return BadRequest();
        }

        await _signInManager.SignOutAsync(); // sign out
        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> CreateRoles() // Bir dəfə istifadə olunur Role`ları yaratmaq üçün
    {
        foreach (var role in Enum.GetValues(typeof(RoleType))) // Eyni role`u təkrar yaratmamaq üçündü bu kod block`u
        {
            if (!await _roleManager.RoleExistsAsync(role.ToString()))
            {

                await _roleManager.CreateAsync(new IdentityRole { Name = role.ToString() });
            }
        }
        return Json("Ok");
    }

    public IActionResult ForgotPassword()
    {

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        var user = await _userManager.FindByEmailAsync(forgotPasswordViewModel.EmailAddress);
        if (user is null)
        {
            ModelState.AddModelError("EmailAddress", "User doesn`t exist");
            return View();
        }

        string token = await _userManager.GeneratePasswordResetTokenAsync(user); // Generate Reset Password Token for user

        // Action       //Controller     //url`nin daxilində olanlar        // https
        string url = Url.Action("ResetPassword", "Account", new { userId = user.Id, token = token }, HttpContext.Request.Scheme); // Reset password üçün url yaradırıq

        // Send email section
        EmailHelper emailHelper = new EmailHelper();

        MailRequestViewModel mailRequestViewModel = new()
        { 
            ToEmail = user.Email,
            Subject = "Reset your password",
            Body = $"<a href='{url}'>Reset your password </a>"
        };

        await emailHelper.SendEmailAsync(mailRequestViewModel);

        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> ResetPassword(ResetViewModel resetViewModel) // Create viewmodel route`dan gələn dəyərləri viewmodel daxilində qarşılayırıq
    {
        if (string.IsNullOrWhiteSpace(resetViewModel.UserId) || resetViewModel.Token is null)
        {
            return BadRequest();
        }

        var user = await _userManager.FindByIdAsync(resetViewModel.UserId);
        if (user is null) return NotFound();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ChangePasswordViewModel changePasswordViewModel, string userId, string token)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        if (string.IsNullOrWhiteSpace(userId) || token is null)
        {
            return BadRequest();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        var identityResult = await _userManager.ResetPasswordAsync(user, token, changePasswordViewModel.Password); // Hansı userin password`unu dəyişdirmək istədiyimizi deyirik

        if (!identityResult.Succeeded) // identityResult Succeeded dəyəri var. 
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View();
        }

        return RedirectToAction(nameof(Login));
    }
}

