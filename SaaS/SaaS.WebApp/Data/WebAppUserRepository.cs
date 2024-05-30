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
            "SELECT U.McUserId as WebAppUserId, U.Password FROM McUsers U WHERE U.Email=@username", new { username });

        if (currentUser == null) throw new KeyNotFoundException($"User with {username} has not been found!");

        if (!PasswordHash.ValidateHash(password, currentUser.Password))
            throw new Exception("Entered password is not a match");

        currentUser = await GetDetailsForUserAsync(currentUser.WebAppUserId);
        return currentUser;
    }

    public async Task<List<WebAppUser>> GetUsersAsync(string search = "")
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        var query = "SELECT U.McUserId as WebAppUserId,U.Fullname,U.Email,U.IsAdmin," +
                    "P.PackageId, P.Name, P.Description, P.Price FROM McUsers U " +
                    "LEFT JOIN McUser2Packages PP on PP.McUserId=U.McUserId " +
                    "LEFT JOIN Packages P on P.PackageId=PP.PackageId ";

        if (!string.IsNullOrEmpty(search)) query += "WHERE U.Fullname LIKE '%" + search + "%' OR U.Email LIKE '%" + search + "%'";

        var grid = await connection.QueryMultipleAsync(query);
        var lookup = new Dictionary<int, WebAppUser>();

        grid.Read<WebAppUser, Package, WebAppUser>((user, package) =>
        {
            if (!lookup.TryGetValue(user.WebAppUserId, out _))
                lookup.Add(user.WebAppUserId, user);

            user.Packages ??= [];

            if (package == null) return user;
            lookup[user.WebAppUserId].Packages.Add(package);

            return user;
        }, splitOn: "PackageId");
        return lookup.Values.ToList();
    }

    public async Task<WebAppUser> GetDetailsForUserAsync(int userId)
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        var query =
            "SELECT U.McUserId as WebAppUserId,U.Fullname,U.Email,U.IsAdmin FROM McUsers U WHERE U.McUserId=@userId;" +
            "SELECT P.PackageId,P.Name,P.Description,P.Price FROM Packages P " +
            "JOIN McUser2Packages PP on PP.PackageId=P.PackageId " +
            "WHERE PP.McUserId=@userId;";
        var result = await connection.QueryMultipleAsync(query, new { userId });
        var currentUser = await result.ReadSingleAsync<WebAppUser>();
        var packages = await result.ReadAsync<Package>();
        currentUser.Packages = packages.ToList();
        return currentUser;
    }

    public async Task<WebAppUser> CreateNewUserAsync(WebAppUser webAppUser)
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();

        var query =
            "INSERT INTO McUsers (FullName, Email, Password, IsAdmin) VALUES (@FullName, @Email, @Password, @IsAdmin); SELECT CAST(SCOPE_IDENTITY() as int)";
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

    public async Task<WebAppUser> GetUserByEmailAsync(string email)
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        var currentUser = await connection.QuerySingleOrDefaultAsync<WebAppUser>(
            "SELECT U.McUserId as WebAppUserId, U.FullName,U.Email, U.Password FROM McUsers U WHERE U.Email=@requestPublisherId", new { requestPublisherId = email });
        return currentUser;
    }
}