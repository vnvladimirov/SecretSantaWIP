namespace SecretSanta.Models.ViewModels;

public class RevealViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string ParticipantName { get; set; } = string.Empty;
    public string? ReceiverName { get; set; }
}
