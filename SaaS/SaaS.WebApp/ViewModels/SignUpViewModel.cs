using System.ComponentModel.DataAnnotations;

namespace SaaS.WebApp.ViewModels;

public class SignUpViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string RepeatPassword { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    public string FullName { get; set; }
}