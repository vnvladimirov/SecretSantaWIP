using System.ComponentModel.DataAnnotations;
namespace SecretSanta.Models;

public class Assignment
{
    [Key]
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int GiverId { get; set; }
    public Participant Giver { get; set; } = null!;
    public int ReceiverId { get; set; }
    public Participant Receiver { get; set; } = null!;
    public int Round { get; set; }

}
