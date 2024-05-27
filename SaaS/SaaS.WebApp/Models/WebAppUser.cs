namespace SaaS.WebApp.Models;

public class WebAppUser
{
    public int WebAppUserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public bool IsAdmin { get; set; }
}