namespace SbtMultiDB.Models.Requests;

public class IResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
}
