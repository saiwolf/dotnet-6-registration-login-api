namespace WebApi.Entities;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class User
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string FirstName { get; set; }
    public string LastName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    [JsonIgnore]
    public string PasswordHash { get; set; }
}