using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Data.Settings;

public class DatabaseSettings
{
    [Required]
    public string? DatabaseConnection { get; set; }
}
