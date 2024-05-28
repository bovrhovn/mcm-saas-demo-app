using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaaS.WebApp.ViewModels;

namespace SaaS.WebApp.Pages.Profile;

[AllowAnonymous]
public class SignUpPageModel(ILogger<SignUpPageModel> logger) : PageModel
{
    public void OnGet()
    {
        logger.LogInformation("Sign up page visited {DateLoaded}", DateTime.Now);
    }
    
    public void OnPost()
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid signup model");
            Message = "Invalid signup - check username and password";
            return;
        }

        logger.LogInformation("Signup attempt for {Email}", SignupModel.Username);
        //sign up
    }
    
    [BindProperty] public SignUpViewModel SignupModel { get; set; } = new();
    [BindProperty]public string Message { get; set; }
    [BindProperty(SupportsGet = true)] public string Token { get; set; }
}