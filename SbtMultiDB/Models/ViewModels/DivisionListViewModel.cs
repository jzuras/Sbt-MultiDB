namespace SbtMultiDB.Models.ViewModels;

public class DivisionListViewModel
{
    public string Organization { get; set; } = string.Empty;

    public IList<Division> DivisionsList { get; set; } = default!;
}
