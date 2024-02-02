using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SbtMultiDB.Models.ViewModels;

public class LoadScheduleViewModel
{
    public string Organization { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[a-zA-Z]+[a-zA-Z0-9-_]*$", ErrorMessage = "Allowed: digits, letters, dash, and underline.")]
    [StringLength(50, MinimumLength = 2)]
    [Remote("DivisionExists", "Admin", ErrorMessage = "Division not found.", 
        AdditionalFields = nameof(Organization))]
    public string Abbreviation { get; set; } = string.Empty;

    [Required]
    [DisplayName("Schedule File")]
    public IFormFile ScheduleFile { get; set; } = default!;

    public bool UsesDoubleHeaders { get; set; } = false;

    public bool ResultSuccess { get; set; } = false;

    public string ResultMessage { get; set; } = string.Empty;
}
