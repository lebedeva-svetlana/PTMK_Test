using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PTMK_Test.Extensions;
using PTMK_Test.Models;
using System.Data;
using System.Data.SqlClient;

namespace PTMK_Test.Services;

public class EmployeesService : IEmployeesService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmployeesService> _logger;

    public EmployeesService(IConfiguration config, ILogger<EmployeesService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private string? GetConnectionString()
    {
        string? connectionString = _config.GetValue<string>("ConnectionStrings:Default");
        if (connectionString is null)
        {
            ArgumentNullException exception = new("connectionString");
            _logger.LogError(exception, "Connection string cannot be null.");
        }
        return connectionString;
    }

    private string? GetQueryString(string key)
    {
        string? query = _config.GetValue<string>($"Queries:{key}");
        if (query is null)
        {
            ArgumentNullException exception = new("query");
            _logger.LogError(exception, "Query string cannot be null.");
        }
        return query;
    }

    public async Task<bool> CreateTable()
    {
        var connectionString = GetConnectionString();
        var query = GetQueryString("CreateTable");

        if (connectionString is null || query is null)
        {
            return false;
        }

        using SqlConnection connection = new(connectionString);
        SqlCommand command = new(query, connection);
        await connection.OpenAsync();

        try
        {
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while executing query.");
            return false;
        }
    }

    private static int GetEmployeeAge(DateTime birthdate)
    {
        DateTime today = DateTime.Today;
        int age = today.Year - birthdate.Year;
        if (birthdate.Date > today.AddYears(-age))
        {
            --age;
        }
        return age;
    }

    private bool AddEmployeesParams(SqlCommand command, Employee employee)
    {
        SqlParameter fullNameParam = new()
        {
            ParameterName = "@FullName",
            SqlDbType = SqlDbType.NVarChar,
            Value = employee.FullName
        };
        SqlParameter birthDateParam = new()
        {
            ParameterName = "@BirthDate",
            SqlDbType = SqlDbType.DateTime2,
            Value = employee.BirthDate
        };
        SqlParameter isMaleParan = new()
        {
            ParameterName = "@IsMale",
            SqlDbType = SqlDbType.Bit,
            Value = employee.IsMale
        };

        try
        {
            command.Parameters.Add(fullNameParam);
            command.Parameters.Add(birthDateParam);
            command.Parameters.Add(isMaleParan);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while adding query parameters.");
            return false;
        }
    }

    public async Task<bool> InsertEmployee(Employee employee)
    {
        var connectionString = GetConnectionString();
        var query = GetQueryString("InsertEmployee");

        if (connectionString is null || query is null)
        {
            return false;
        }

        using SqlConnection connection = new(connectionString);
        SqlCommand command = new(query, connection);

        bool isSuccess = AddEmployeesParams(command, employee);
        if (!isSuccess)
        {
            return false;
        }

        await connection.OpenAsync();

        try
        {
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while executing query.");
            return false;
        }
    }

    public async Task<IList<Employee>?> SelectAllEmployees()
    {
        var connectionString = GetConnectionString();
        var query = GetQueryString("SelectAllEmployees");

        if (connectionString is null || query is null)
        {
            return null;
        }

        using SqlConnection connection = new(connectionString);
        SqlCommand command = new(query, connection);

        await connection.OpenAsync();

        try
        {
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                return null;
            }

            List<Employee> employees = new();

            while (await reader.ReadAsync())
            {
                string fullName = reader.GetString(0);
                DateTime birthDate = reader.GetDateTime(1);
                bool isMale = reader.GetBoolean(2);

                employees.Add(
                    new Employee()
                    {
                        FullName = fullName,
                        BirthDate = birthDate,
                        Age = GetEmployeeAge(birthDate),
                        IsMale = isMale
                    }
                );
            }

            return employees;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while executing query.");
            return null;
        }
    }

    public async Task<bool> InsertEmployees(IEnumerable<IEmployeeBase> employees)
    {
        var connectionString = GetConnectionString();
        if (connectionString is null)
        {
            return false;
        }

        using SqlConnection connection = new(connectionString);
        using SqlBulkCopy bulkCopy = new(connection);

        bulkCopy.DestinationTableName = "Employees";
        bulkCopy.ColumnMappings.Add("FullName", "FullName");
        bulkCopy.ColumnMappings.Add("BirthDate", "BirthDate");
        bulkCopy.ColumnMappings.Add("IsMale", "IsMale");

        await connection.OpenAsync();

        try
        {
            await bulkCopy.WriteToServerAsync(employees.ToDataTable());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while executing query.");
            return false;
        }
    }

    public async Task<(long, IList<Employee>?)> SelectAllFMan()
    {
        var connectionString = GetConnectionString();
        var query = GetQueryString("SelectAllFMan");

        if (connectionString is null || query is null)
        {
            return (default, null);
        }

        using SqlConnection connection = new(connectionString);
        SqlCommand command = new(query, connection);

        connection.StatisticsEnabled = true;
        await connection.OpenAsync();

        try
        {
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                return (default, null);
            }

            List<Employee> employees = new();

            while (await reader.ReadAsync())
            {
                string fullName = reader.GetString(0);
                DateTime birthDate = reader.GetDateTime(1);
                bool isMale = reader.GetBoolean(2);

                employees.Add(
                    new Employee()
                    {
                        FullName = fullName,
                        BirthDate = birthDate,
                        Age = GetEmployeeAge(birthDate),
                        IsMale = isMale
                    }
                );
            }

            var stats = connection.RetrieveStatistics();
            long milliseconds = (long)stats["ExecutionTime"];

            return (milliseconds, employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while executing query.");
            return (default, null);
        }
    }
}