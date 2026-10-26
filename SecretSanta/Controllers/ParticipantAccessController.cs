
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
    public IActionResult Welcome()
    {
        SetPrivateResponseHeaders();
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
