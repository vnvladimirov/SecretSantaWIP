namespace SecretSanta.Models;
using System.ComponentModel.DataAnnotations;

public class WishlistItem
{
    public int Id { get; set; }
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    [Url]
    [MaxLength(2048)]
    public string? ProductURL { get; set; }

    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;
}