namespace PTMK_Test.Models;

public class Employee : IEmployeeBase
{
    public string FullName { get; set; }

    public DateTime BirthDate { get; set; }

    public int Age { get; set; }

    public bool IsMale { get; set; }
}