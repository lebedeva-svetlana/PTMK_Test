using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Logging;
using PTMK_Test.Models;
using System.Globalization;
using System.Text;

namespace PTMK_Test.Services;

public class CSVEmployeesService : IDataEmployeesService
{
    private readonly ILogger<CSVEmployeesService> _logger;

    public CSVEmployeesService(ILogger<CSVEmployeesService> logger)
    {
        _logger = logger;
    }

    public (bool, IEnumerable<IEmployeeBase>?) GetEmployees(string fileName)
    {
        IList<IEmployeeBase> employees = new List<IEmployeeBase>();
        StringBuilder errorBuilder = new();

        CsvConfiguration configuration = new(CultureInfo.InvariantCulture)
        {
            Encoding = Encoding.UTF8,
            Delimiter = ";",
            HasHeaderRecord = true
        };

        using var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8);

        TypeConverterOptions options = new() { Formats = new[] { "dd-MM-yyyy" } };
        using CsvReader csvReader = new(streamReader, configuration);
        csvReader.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);

        while (csvReader.Read())
        {
            try
            {
                var record = csvReader.GetRecord<EmployeeBase>();
                employees.Add(record);
            }
            catch (Exception ex)
            {
                errorBuilder.AppendLine(ex.Message);
            }
        }

        if (errorBuilder.Length > 0)
        {
            _logger.LogError($"{fileName} exceptions:\n\n{errorBuilder}");
            return (false, null);
        }
        else
        {
            return (true, employees);
        }
    }
}