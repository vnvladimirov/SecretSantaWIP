using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SecretSanta.Models;

public class Participant
{
    [Key]
    public int Id { get; set; }
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int? PartnerId { get; set; }
    public Participant? Partner { get; set; }

}
