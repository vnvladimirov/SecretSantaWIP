using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Data;

namespace SecretSanta.Controllers;

public class WishlistItemController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index(int participantId)
    {
        var participant = await _context.Participants
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == participantId);

        if (participant == null)
        {
            return NotFound();
        }
        ViewBag.Participant = participant;

        var items = await _context.WishlistItems
        .AsNoTracking()
        .Where(w => w.ParticipantId == participantId)
        .OrderBy(w => w.Title)
        .ToListAsync();

        return View(items);
    }
}
