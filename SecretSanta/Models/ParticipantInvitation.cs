using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Models;

public class ParticipantInvitation
{
    public int Id { get; set; }
    [Required]
    [MaxLength(64)]
    public string TokenHash { get; set; } = string.Empty;
    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }

}
