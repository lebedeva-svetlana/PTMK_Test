namespace PTMK_Test.Models;

public class EmployeeBase : IEmployeeBase
{
    public string FullName { get; set; }

    public DateTime BirthDate { get; set; }

    public bool IsMale { get; set; }
}