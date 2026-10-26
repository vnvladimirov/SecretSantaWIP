using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Data;
using SecretSanta.Security;
using SecretSanta.Models;
using NetTopologySuite.Index.HPRtree;

namespace SecretSanta.Controllers;

[Authorize(AuthenticationSchemes = ParticipantCookieDefaults.AuthenticationScheme)]
public class MyWishlistController : Controller
{
    private readonly ApplicationDbContext _context;

    public MyWishlistController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int? CurrentParticipantId
    {
        get
        {
            var value = User.FindFirst(ParticipantCookieDefaults.ParticipantIdClaim)?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    
    public async Task<IActionResult> Index()
    {
        var participantId = CurrentParticipantId;

        if (participantId == null)
        {
            return Challenge(ParticipantCookieDefaults.AuthenticationScheme);
        }

        var items = await _context.WishlistItems
        .Where(w => w.ParticipantId == participantId.Value)
        .OrderBy(w => w.Title)
        .ToListAsync();

        return View(items);
    }

    public IActionResult Create()
    {
        if (CurrentParticipantId == null)
        {
            return Challenge(ParticipantCookieDefaults.AuthenticationScheme);
        }

        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,ProductUrl")] WishlistItem item)
    {
        var participantId = CurrentParticipantId;

        if (participantId == null)
        {
            return Challenge(ParticipantCookieDefaults.AuthenticationScheme);
        }

        if (!ModelState.IsValid)
        {
            return View(item);
        }

        item.ParticipantId = participantId.Value;

        _context.WishlistItems.Add(item);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
