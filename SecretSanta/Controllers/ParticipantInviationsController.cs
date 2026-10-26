using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Data;
using SecretSanta.Models;
using SecretSanta.Security;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SecretSanta.Controllers;

[Authorize]
public class ParticipantInvitationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ParticipantInvitationsController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    private string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static string HashToken(string token)
    {
        byte[] hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(hash);
    }

    private void SetPrivateResponseHeaders()
    {
        Response.Headers["Cache-Control"] = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(int participantId)
    {
        var participant = await _context.Participants
            .Include(p => p.Event)
            .FirstOrDefaultAsync(p => p.Id == participantId);

        if (participant == null)
        {
            return NotFound();
        }

        if (participant.Event.CreatorId != CurrentUserId)
        {
            return NotFound();
        }

        var previousInvitations = await _context.ParticipantInvitations
            .Where(i =>
                i.ParticipantId == participant.Id &&
                i.RevokedAtUtc == null)
            .ToListAsync();

        foreach (var previous in previousInvitations)
        {
            previous.RevokedAtUtc = DateTime.UtcNow;
        }


        string token = WebEncoders.Base64UrlEncode(
            RandomNumberGenerator.GetBytes(32));

        var invitation = new ParticipantInvitation
        {
            ParticipantId = participant.Id,
            TokenHash = HashToken(token),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };

        _context.ParticipantInvitations.Add(invitation);

        await _context.SaveChangesAsync();

        string? invitationUrl = Url.Action(
            nameof(ParticipantAccessController.Enter),
            "ParticipantAccess",
            new { token },
            Request.Scheme);

        if (invitationUrl == null)
        {
            return StatusCode(500);
        }

        SetPrivateResponseHeaders();
        ViewBag.EventId = participant.EventId;
        ViewBag.ParticipantName = participant.Name;
        ViewBag.EventName = participant.Event.Name;
        ViewBag.InvitationUrl = invitationUrl;
        ViewBag.ExpiresAt = invitation.ExpiresAtUtc;

        return View("Created");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(int id)
    {
        var invitation = await _context.ParticipantInvitations
            .Include(i => i.Participant)
            .ThenInclude(p => p.Event)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invitation == null)
        {
            return NotFound();
        }

        if (invitation.Participant.Event.CreatorId != CurrentUserId)
        {
            return NotFound();
        }

        if (invitation.RevokedAtUtc == null)
        {
            invitation.RevokedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(
            "Details",
            "Events",
            new { id = invitation.Participant.EventId });
    }
}
