using SbtMultiDB.Models.Requests;
using System.ComponentModel.DataAnnotations;

namespace SbtMultiDB.Models.ViewModels;

public class ScoresViewModel
{
    public string Organization { get; set; } = default!;

    public string Abbreviation { get; set; } = default!;

    public IList<ScheduleSubsetForScoresViewModel> Schedule { get; set; } = default!;

    public ScoresViewModel() { }

    public ScoresViewModel(IList<Schedule> schedule, string organization, string abbreviation)
    {
        this.Organization = organization;
        this.Abbreviation = abbreviation;
        this.Schedule = new List<ScheduleSubsetForScoresViewModel>();
        for (int i = 0; i < schedule.Count; i++)
        {
            var subset = new ScheduleSubsetForScoresViewModel
            {
                GameID = schedule[i].GameID,
                HomeScore = schedule[i].HomeScore,
                VisitorScore = schedule[i].VisitorScore,
                HomeForfeit = schedule[i].HomeForfeit,
                VisitorForfeit = schedule[i].VisitorForfeit,
                Home = schedule[i].Home,
                Visitor = schedule[i].Visitor,
                Day = schedule[i].Day,
                Time = schedule[i].Time,
                Field = schedule[i].Field,
                OvertimeGame = schedule[i].OvertimeGame,
            };
            this.Schedule.Add(subset);
        }
    }

    public UpdateScoresRequest ToScoresRequest()
    {
        var request = new UpdateScoresRequest
        {
            Organization = this.Organization,
            Abbreviation = this.Abbreviation,
            Scores = this.Schedule.Select(game => new ScheduleSubsetForUpdateScoresRequest 
            { 
                GameID = game.GameID,
                HomeScore = game.HomeScore,
                VisitorScore = game.VisitorScore,
                HomeForfeit =  game.HomeForfeit,
                VisitorForfeit = game.VisitorForfeit
            }).ToList(),
        };

        return request;
    }

    public bool IsValid(out string errorMessage)
    {
        // Check that forfeit scores are 7-0 or 0-0 if checkboxes are selected.

        int gameNumber = 0;
        errorMessage = "";

        foreach (var game in this.Schedule)
        {
            gameNumber++;
            if (game.HomeForfeit)
            {
                if (game.VisitorForfeit)
                {
                    if (game.HomeScore != 0 || game.VisitorScore != 0)
                    {
                        errorMessage += $"Score must be 0-0 for double forfeit for Game #{gameNumber}. ";
                    }
                }
                else
                {
                    if (game.HomeScore != 0 || game.VisitorScore != 7)
                    {
                        errorMessage += $"Score must be 7-0 for forfeit for Game #{gameNumber}. ";
                    }
                }
            }
            else if (game.VisitorForfeit)
            {
                if (game.HomeScore != 7 || game.VisitorScore != 0)
                {
                    errorMessage += $"Score must be 7-0 for forfeit for Game #{gameNumber}. ";
                }
            }
        }

        return (errorMessage == "");
    }
}

public class ScheduleSubsetForScoresViewModel
{
    public int GameID { get; set; }

    public string Home { get; set; } = string.Empty;

    public string Visitor { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? Day { get; set; }

    public DateTime? Time { get; set; }

    public string Field { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid integer value.")]
    public short? HomeScore { get; set; }

    [Required]
    [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid integer value.")]
    public short? VisitorScore { get; set; }

    public bool HomeForfeit { get; set; }

    public bool VisitorForfeit { get; set; }

    public bool OvertimeGame { get; set; }
}