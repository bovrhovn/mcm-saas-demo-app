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

    public async Task<List<Package>> GetPackagesAsync()
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        var query = "SELECT P.PackageId,P.Name,P.Price,P.Description FROM Packages P;";
        var packages = await connection.QueryAsync<Package>(query);
        return packages.ToList();
    }
    
    public async Task<Package> AddPackageAsync(Package package)
    {
        await using var connection = new SqlConnection(sqlOptions.ConnectionString);
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();
        var query = "INSERT INTO Packages (Name,Price,Description) VALUES (@name, @price, @desc); SELECT CAST(SCOPE_IDENTITY() as int)";
        var packageId = await connection.QuerySingleAsync<int>(query, new
        {
            name = package.Name,
            price = package.Price,
            desc = package.Description
        });
        package.PackageId = packageId;
        return package;
    }
}