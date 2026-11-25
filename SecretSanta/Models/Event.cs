using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
namespace SecretSanta.Models;

public class Event
{
    [Key]
    public int Id { get; set; }
    [Required]
    [MinLength(3)]
    [MaxLength(120)]
    [Display(Name = "Event name")]
    public string Name { get; set; } = string.Empty;
    [MaxLength(200)]
    public string? Description { get; set; }
    [DataType(DataType.Date)]
    [Display(Name = "Event date")]
    public DateTime EventDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.0, 100000.0, ErrorMessage = "The gift budget must be between 0 and 100000.")]
    [Display(Name = "Gift budget")]
    public decimal Budget { get; set; }
    [Required]
    [Range(6,50)]
    [Display(Name = "Minimum participants")]
    public int MinParticipants { get; set; }
    [Required]
    [MaxLength(50)]
    [Display(Name = "Organizer name")]
    public string OrganizerName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    [Display(Name = "Organizer email")]
    public string OrganizerEmail { get; set; } = string.Empty;

    public string? CreatorId { get; set; }

    public IdentityUser? Creator { get; set; }
    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

}
