using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SaaS.WebApp.Data;

public static class UserDataContextExtensions
{
    public static ClaimsPrincipal GenerateClaims(this Models.WebAppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.NameIdentifier, user.WebAppUserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.IsAdmin.ToString())
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }
}