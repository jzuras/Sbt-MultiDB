namespace SbtMultiDB.Models.Requests;

public class IRequest
{
    public string Organization { get; set; } = default!;
    public string Abbreviation { get; set; } = default!;
}