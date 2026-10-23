namespace SecretSanta.Services;


public record DrawResult(bool Success, string? Error = null);
public interface IDrawService
{
    Task<DrawResult> DrawAsync(int eventId);
}
