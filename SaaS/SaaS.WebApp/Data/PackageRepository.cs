using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Options;
using SaaS.WebApp.Models;
using SaaS.WebApp.Options;

namespace SaaS.WebApp.Data;

public class PackageRepository(IOptions<SqlOptions> sqlOptionsValue)
{
    private readonly SqlOptions sqlOptions = sqlOptionsValue.Value;

    public async Task<List<Package>> GetSubscriptionsAsync(int userId)
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        var query = "SELECT P.PackageId,P.Name,P.Price,P.Description FROM McUser2Packages PP " +
                    "INNER JOIN Packages P ON PP.PackageId=P.PackageId WHERE PP.McUserId=@userId";
        var packages = await connection.QueryAsync<Package>(query, new {userId});
        return packages.ToList();
    }
    
    public async Task<bool> SubscribeToPackageAsync(int packageId, int userId)
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        
        //check if user is already subscribed to the package
        var query = "SELECT COUNT(*) FROM McUser2Packages WHERE McUserId=@userId AND PackageId=@packageId";
        var count = await connection.QuerySingleAsync<int>(query, new {userId, packageId});
        if (count > 0) return true;
        
        //subscribe user to the package
        query = "INSERT INTO McUser2Packages (McUserId, PackageId,CreatedAt) VALUES (@userId, @packageId, @createdAt)";
        var result = await connection.ExecuteAsync(query, new {userId, packageId, createdAt = DateTime.Now});
        return result > 0;
    }
    
    public async Task<List<Package>> GetPackagesAsync()
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        var query = "SELECT P.PackageId,P.Name,P.Price,P.Description," +
                    "UserCount=" +
                    "(SELECT count(PP.McUserId) FROM McUser2Packages PP WHERE PP.PackageId=P.PackageId) " +
                    "FROM Packages P";
        var packages = await connection.QueryAsync<Package>(query);
        return packages.ToList();
    }

    public async Task<Package> AddPackageAsync(Package package)
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        var query =
            "INSERT INTO Packages (Name,Price,Description) VALUES (@name, @price, @desc); SELECT CAST(SCOPE_IDENTITY() as int)";
        var packageId = await connection.QuerySingleAsync<int>(query, new
        {
            name = package.Name,
            price = package.Price,
            desc = package.Description
        });
        package.PackageId = packageId;
        return package;
    }

    public async Task<List<Package>> SearchAsync(string query)
    {
        if (string.IsNullOrEmpty(query)) return await GetPackagesAsync();

        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        var currentQuery = "SELECT P.PackageId,P.Name,P.Price,P.Description," +
                           "UserCount=" +
                           "(SELECT count(PP.McUserId) FROM McUser2Packages PP WHERE PP.PackageId=P.PackageId)  " +
                           "FROM Packages P WHERE P.Name LIKE '%" + query + "%';";
        var packages = await connection.QueryAsync<Package>(currentQuery);
        return packages.ToList();
    }

    public async Task<bool> UnSubscribeFromPackageAsync(int packageId, int currentUserUserId)
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        var query = "DELETE FROM McUser2Packages WHERE McUserId=@userId AND PackageId=@packageId";
        var result = await connection.ExecuteAsync(query, new {userId = currentUserUserId, packageId});
        return result > 0;
    }
}