namespace SbtMultiDB.Models.ViewModels;

public class StandingsViewModel
{
    public SbtMultiDB.Models.Division Division { get; set; } = new Division();

    public bool ShowOvertimeLosses { get; set; } = false;

    public string? TeamName { get; set; }

}
