using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace SecretSanta.Models;


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
    public string? ProductUrl { get; set; }

    public int ParticipantId { get; set; }
    
    [ValidateNever]
    public Participant Participant { get; set; } = null!;
}