using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaaS.WebApp.Data;
using SaaS.WebApp.ViewModels;

namespace SaaS.WebApp.Pages.Profile;

[AllowAnonymous]
public class LoginPageModel(ILogger<LoginPageModel> logger, WebAppUserRepository webAppUserRepository) : PageModel
{
    public void OnGet() => logger.LogInformation("Login page visited {DateLoaded}", DateTime.Now);
    
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid login model");
            Message = "Invalid login - check username and password";
            return Page();
        }

        logger.LogInformation("Login attempt for {Email}", LoginModel.Username);
        try
        {
            var user = await webAppUserRepository.LoginAsync(LoginModel.Username, LoginModel.Password);
            await HttpContext.SignInAsync(user.GenerateClaims());
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            Message = e.Message;
            return Page();
        }

        return RedirectToPage("/Profile/My");
    }

    [BindProperty] public LoginViewModel LoginModel { get; set; } = new();
    [BindProperty]public string Message { get; set; }
}