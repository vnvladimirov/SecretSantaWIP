
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Data;
using SecretSanta.Security;

namespace SecretSanta.Controllers;

[Authorize(AuthenticationSchemes = ParticipantCookieDefaults.AuthenticationScheme)]
public class ParticipantAccessController : Controller
{
    private readonly ApplicationDbContext _context;

    public ParticipantAccessController(ApplicationDbContext context)
    {
        _context = context;
    }

    private static string HashToken(string token)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    private async Task<Models.ParticipantInvitation?> FindValidInvitation(
        string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        string tokenHash = HashToken(token);

        var invitation = await _context.ParticipantInvitations
            .Include(i => i.Participant)
            .ThenInclude(p => p.Event)
            .FirstOrDefaultAsync(i => i.TokenHash == tokenHash);

        if (invitation == null ||
            invitation.RevokedAtUtc != null ||
            invitation.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return null;
        }

        return invitation;
    }

    private void SetPrivateResponseHeaders()
    {
        Response.Headers["Cache-Control"] = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Enter(string? token)
    {
        SetPrivateResponseHeaders();

        var invitation = await FindValidInvitation(token);

        if (invitation == null)
        {
            return View("InvalidInvitation");
        }

        ViewBag.ParticipantName = invitation.Participant.Name;
        ViewBag.EventName = invitation.Participant.Event.Name;

        return View(model: token);
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterPost(string token)
    {
        SetPrivateResponseHeaders();

        var invitation = await FindValidInvitation(token);

        if (invitation == null)
        {
            return View("InvalidInvitation");
        }

        var claims = new List<Claim>
        {
            new(ParticipantCookieDefaults.ParticipantIdClaim, invitation.ParticipantId.ToString()),
            new(ParticipantCookieDefaults.EventIdClaim, invitation.Participant.EventId.ToString()),
            new(ParticipantCookieDefaults.InvitationIdClaim, invitation.Id.ToString()),
            new(ClaimTypes.Name, invitation.Participant.Name)
        };

        var identity = new ClaimsIdentity(
            claims,
            ParticipantCookieDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = false,
            AllowRefresh = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            ParticipantCookieDefaults.AuthenticationScheme,
            principal,
            properties);

        return RedirectToAction(nameof(Welcome));
    }

    [Authorize(AuthenticationSchemes = ParticipantCookieDefaults.AuthenticationScheme)]
    [HttpGet]
    public async Task<IActionResult> Welcome()
    {
            SetPrivateResponseHeaders();

            var participantIdValue = User.FindFirstValue(
                ParticipantCookieDefaults.ParticipantIdClaim);

            var eventIdValue = User.FindFirstValue(
                ParticipantCookieDefaults.EventIdClaim);

            if (!int.TryParse(participantIdValue, out var participantId) ||
                !int.TryParse(eventIdValue, out var eventId))
            {
                return Forbid();
            }

            var participant = await _context.Participants
                .AsNoTracking()
                .Include(p => p.Event)
                .FirstOrDefaultAsync(p =>
                    p.Id == participantId &&
                    p.EventId == eventId);

            if (participant == null)
                return NotFound();

            ViewBag.Participant = participant;
            ViewBag.EventName = participant.Event.Name;

            var latestRound = await _context.Assignments
                .Where(a => a.EventId == eventId)
                .Select(a => (int?)a.Round)
                .MaxAsync();

            if (latestRound.HasValue)
            {
                var assignment = await _context.Assignments
                    .AsNoTracking()
                    .Where(a =>
                        a.EventId == eventId &&
                        a.Round == latestRound.Value &&
                        a.GiverId == participantId)
                    .Select(a => new
                    {
                        a.ReceiverId,
                        ReceiverName = a.Receiver.Name
                    })
                    .FirstOrDefaultAsync();

                if (assignment != null)
                {
                    ViewBag.ReceiverId = assignment.ReceiverId;
                    ViewBag.ReceiverName = assignment.ReceiverName;
                }
            }

            return View();
        }

    [Authorize(AuthenticationSchemes = ParticipantCookieDefaults.AuthenticationScheme)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            ParticipantCookieDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Enter));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Denied()
    {
        return View();
    }
}
