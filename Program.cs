using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PTMK_Test.Models;
using PTMK_Test.Services;
using Serilog;

if (args.Length == 0)
{
    Console.WriteLine("Please enter a numeric argument.\n\n1 — Create an employee table.\n2 — Add an employee to the table. Required data: full name, date of birth and sex. Example: 2 'Ivanov Petr Sergeevich' 2009-07-12 Male\n3 — Get all employees.\n4 — Add example employees to the table.\n5 — Get time in milliseconds of selecting all male employees whose last name begins with the letter F.");
    return;
}

var hostBuilder = Host.CreateApplicationBuilder(args);
hostBuilder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(hostBuilder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

hostBuilder.Logging.ClearProviders();
hostBuilder.Logging.AddSerilog(logger);
hostBuilder.Services.BuildServiceProvider();

hostBuilder.Services.AddScoped<IEmployeesService, EmployeesService>();
hostBuilder.Services.AddScoped<IDataEmployeesService, CSVEmployeesService>();

var host = hostBuilder.Build();

var employeesService = host.Services.GetService<IEmployeesService>();
var employeesDataService = host.Services.GetService<IDataEmployeesService>();

switch (args[0])
{
    case "1":
        await CreateTable();
        break;

    case "2":
        await InsertEmployee(args);
        break;

    case "3":
        await SelectAllEmployees();
        break;

    case "4":
        await InsertExampleEmployees();
        break;

    case "5":
        await SelectAllFMan();
        break;
}

async Task SelectAllFMan()
{
    (long time, var employees) = await employeesService.SelectAllFMan();
    if (employees is null)
    {
        Console.WriteLine("There are no male employees whose last name begins with the letter F in the database.");
        return;
    }
    Console.WriteLine($"Employee count: {employees.Count}. Execution time in milliseconds: {time}");
    Console.WriteLine("Completed successfully.");
}

async Task CreateTable()
{
    bool isSuccess = await employeesService.CreateTable();
    if (!isSuccess)
    {
        Console.WriteLine("Something went wrong with creating table.");
    }
    Console.WriteLine("Completed successfully.");
}

async Task InsertExampleEmployees()
{
    (bool isFMaleSuccess, var fMaleEmployees) = employeesDataService.GetEmployees("Data/1000FMaleEmployees.csv");
    if (!isFMaleSuccess)
    {
        Console.WriteLine("Something went wrong with reading 1000FMaleEmployees.csv.");
        return;
    }

    (bool isAllSuccess, var allEmployees) = employeesDataService.GetEmployees("Data/1000000Employees.csv");
    if (!isAllSuccess)
    {
        Console.WriteLine("Something went wrong with reading 1000000Employees.csv.");
        return;
    }

    isAllSuccess = await employeesService.InsertEmployees(allEmployees);
    if (!isAllSuccess)
    {
        Console.WriteLine("Something went wrong with inserting example 1000000Employees.");
        return;
    }

    isFMaleSuccess = await employeesService.InsertEmployees(fMaleEmployees);
    if (!isFMaleSuccess)
    {
        Console.WriteLine("Something went wrong with inserting example 1000FMaleEmployees.");
        return;
    }
    Console.WriteLine("Completed successfully.");
}

async Task InsertEmployee(string[] args)
{
    if (args.Length < 4)
    {
        Console.WriteLine("Required arguments are missing. Example: 2 'Ivanov Petr Sergeevich' 2009-07-12 Male");
        return;
    }

    Employee employee = new();
    employee.FullName = args[1];

    bool isSuccess = DateTime.TryParse(args[2], out DateTime birthDay);
    if (!isSuccess)
    {
        Console.WriteLine("The date was entered in an incorrect format.");
        return;
    }
    employee.BirthDate = birthDay;

    args[3] = args[3].ToLower();
    string male = "male";
    string female = "female";

    if (args[3] != male && args[3] != female)
    {
        Console.WriteLine("The sex was entered in an incorrect format. Enter 'male' or 'female'.");
        return;
    }

    bool isMale = args[3] == male;
    employee.IsMale = isMale;

    isSuccess = await employeesService.InsertEmployee(employee);
    if (!isSuccess)
    {
        Console.WriteLine("Something went wrong with inserting employee.");
        return;
    }
    Console.WriteLine("Completed successfully.");
}

async Task SelectAllEmployees()
{
    var employees = (List<Employee>?)await employeesService.SelectAllEmployees();
    if (employees is null)
    {
        Console.WriteLine("There are no employees in the database.");
        return;
    }
    for (int i = 0; i < employees.Count; ++i)
    {
        string sex = employees[i].IsMale ? "Male" : "Female";
        Console.WriteLine($"{i + 1}. {employees[i].FullName} {employees[i].BirthDate:yyyy-MM-dd} {sex} {employees[i].Age}");
    }
    Console.WriteLine("Completed successfully.");
}