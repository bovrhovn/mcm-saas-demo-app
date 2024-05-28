using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Options;
using SaaS.WebApp.Models;
using SaaS.WebApp.Options;

namespace SaaS.WebApp.Data;

public class WebAppUserRepository(IOptions<SqlOptions> sqlOptionsValue)
{
    private readonly SqlOptions sqlOptions = sqlOptionsValue.Value;

    public async Task<WebAppUser> LoginAsync(string username, string password)
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        
        var currentUser = await connection.QuerySingleOrDefaultAsync<WebAppUser>(
            "SELECT U.McUserId, U.Password, U.Email, U.IsAdmin, U.Fullname FROM McUsers U WHERE U.Email=@username", new { username });

        if (currentUser == null) throw new KeyNotFoundException($"User with {username} has not been found!");

        if (!PasswordHash.ValidateHash(password, currentUser.Password))
            throw new Exception("Entered password is not a match");

        return currentUser;
    }

    public async Task<WebAppUser> CreateNewUserAsync(WebAppUser webAppUser)
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        
        var query = "INSERT INTO McUsers (FullName, Email, Password, IsAdmin) VALUES (@FullName, @Email, @Password, @IsAdmin); SELECT CAST(SCOPE_IDENTITY() as int)";
        var webAppUserId = await connection.QuerySingleAsync<int>(query, new
        {
            FullName = webAppUser.FullName,
            Email = webAppUser.Email,
            Password = PasswordHash.CreateHash(webAppUser.Password),
            IsAdmin = webAppUser.IsAdmin
        });
        webAppUser.WebAppUserId = webAppUserId;
        return webAppUser;
    }
}