using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SecretSanta.Models;

public class Event
{
    [Key]
    public int Id { get; set; }
    [Required]
    [MinLength(3)]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(200)]
    public string? Description { get; set; }
    [DataType(DataType.Date)]
    public DateTime EventDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Budget { get; set; }
    [Required]
    [Range(6,50)]
    public int MinParticipants { get; set; }
    [Required]
    [MaxLength(50)]
    public string OrganizerName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [MaxLength(50)]
    public string OrganizerEmail { get; set; } = string.Empty;

    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();

}
