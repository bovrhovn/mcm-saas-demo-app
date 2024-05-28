using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaaS.WebApp.ViewModels;

namespace SaaS.WebApp.Pages.Profile;

[AllowAnonymous]
public class LoginPageModel(ILogger<LoginPageModel> logger) : PageModel
{
    public void OnGet() => logger.LogInformation("Login page visited {DateLoaded}", DateTime.Now);
    
    public void OnPost()
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid login model");
            Message = "Invalid login - check username and password";
            return;
        }

        logger.LogInformation("Login attempt for {Email}", LoginModel.Username);
        
    }

    [BindProperty] public LoginViewModel LoginModel { get; set; } = new();
    [BindProperty]public string Message { get; set; }
}