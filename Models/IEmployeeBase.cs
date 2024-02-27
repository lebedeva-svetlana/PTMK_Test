namespace PTMK_Test.Models;

public interface IEmployeeBase
{
    public string FullName { get; set; }

    public DateTime BirthDate { get; set; }

    public bool IsMale { get; set; }
}