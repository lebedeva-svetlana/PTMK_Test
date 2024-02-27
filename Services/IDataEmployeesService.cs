using PTMK_Test.Models;

namespace PTMK_Test.Services;

public interface IDataEmployeesService
{
    public (bool, IEnumerable<IEmployeeBase>?) GetEmployees(string fileName);
}