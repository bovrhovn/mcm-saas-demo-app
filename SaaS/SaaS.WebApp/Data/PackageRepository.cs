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
        var query = "SELECT P.PackageId,P.Name,P.Price,P.Description," +
                    "PaymentConfigurationCount=" +
                    "(SELECT count(PP.PaymentId) FROM Package2Payments PP WHERE PP.PackageId=P.PackageId) " +
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
                           "PaymentConfigurationCount=" +
                           "(SELECT count(PP.PaymentId) FROM Package2Payments PP WHERE PP.PackageId=P.PackageId)  " +
                           "FROM Packages P WHERE P.Name LIKE '%" +
                           query + "%';";
        var packages = await connection.QueryAsync<Package>(currentQuery);
        return packages.ToList();
    }
}