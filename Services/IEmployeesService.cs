using PTMK_Test.Models;

namespace PTMK_Test.Services;

public interface IEmployeesService
{
    public Task<bool> CreateTable();

    public Task<bool> InsertEmployee(Employee employee);

    public Task<bool> InsertEmployees(IEnumerable<IEmployeeBase> employees);

    public Task<IList<Employee>?> SelectAllEmployees();

    public Task<(long, IList<Employee>?)> SelectAllFMan();
}