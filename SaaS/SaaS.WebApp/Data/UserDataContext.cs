using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using SaaS.WebApp.ViewModels;

namespace SaaS.WebApp.Data;

public class UserDataContext(IHttpContextAccessor httpContextAccessor)
{
    public UserViewModel GetCurrentUser()
    {
        var httpContextUser = httpContextAccessor.HttpContext?.User;

        var currentUser = new UserViewModel();
        var claimName = httpContextUser?.FindFirst(ClaimTypes.Name);
        currentUser.Fullname = claimName?.Value ?? string.Empty;

        var claimId = httpContextUser?.FindFirst(ClaimTypes.NameIdentifier);
        currentUser.UserId = claimId?.Value ?? string.Empty;

        var claimEmail = httpContextUser?.FindFirst(ClaimTypes.Email);
        currentUser.Email = claimEmail?.Value ?? string.Empty; 
        
        var claimIsAdmin = httpContextUser?.FindFirst(ClaimTypes.Role);
        currentUser.IsAdmin = claimIsAdmin != null && Convert.ToBoolean(claimIsAdmin.Value);

        return currentUser;
    }

    public Task LogOut()
    {
        if (httpContextAccessor.HttpContext != null) 
            return httpContextAccessor.HttpContext.SignOutAsync();
        throw new NotImplementedException("User is not signed in. Report error to IT administrator.");
    }
}

