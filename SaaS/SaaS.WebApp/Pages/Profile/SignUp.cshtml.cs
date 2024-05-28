using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SaaS.WebApp.Data;
using SaaS.WebApp.Models;
using SaaS.WebApp.ViewModels;

namespace SaaS.WebApp.Pages.Profile;

[AllowAnonymous]
public class SignUpPageModel(ILogger<SignUpPageModel> logger, WebAppUserRepository webAppUserRepository) : PageModel
{
    public void OnGet()
    {
        logger.LogInformation("Sign up page visited {DateLoaded}", DateTime.Now);
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid signup model");
            Message = "Invalid signup - check username, password and full name fields";
            return Page();
        }

        logger.LogInformation("Signup attempt for {Email}", SignupModel.Username);
        //sign up
        try
        {
            var user = await webAppUserRepository.CreateNewUserAsync(new WebAppUser
            {
                FullName = SignupModel.FullName,
                Email = SignupModel.Username,
                Password = SignupModel.Password,
                IsAdmin = false
            });
            await HttpContext.SignInAsync(user.GenerateClaims());
            return Redirect("/");
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return Page();
        }
    }
    
    [BindProperty] public SignUpViewModel SignupModel { get; set; } = new();
    [BindProperty]public string Message { get; set; }
    [BindProperty(SupportsGet = true)] public string Token { get; set; }
}