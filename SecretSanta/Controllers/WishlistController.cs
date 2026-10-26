
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Data;
using SecretSanta.Models;

namespace SecretSanta.Controllers;

[Authorize]
public class WishlistController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    private string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    private async Task<bool> OwnsParticipantEvent(int participantId)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrEmpty(userId))
            return false;

        return await _context.Participants
            .AnyAsync(p =>
                p.Id == participantId &&
                p.Event.CreatorId == userId);
    }

    [HttpGet]
    public async Task<IActionResult> Index(int participantId)
    {
        if (!await OwnsParticipantEvent(participantId))
            return NotFound();

        var participant = await _context.Participants
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == participantId);

        if (participant == null)
            return NotFound();

        ViewBag.Participant = participant;

        var items = await _context.WishlistItems
            .AsNoTracking()
            .Where(w => w.ParticipantId == participantId)
            .OrderBy(w => w.Title)
            .ToListAsync();

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int participantId)
    {
        if (!await OwnsParticipantEvent(participantId))
            return NotFound();

        return View(new WishlistItem { ParticipantId = participantId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,ProductUrl,ParticipantId")]
        WishlistItem input)
    {
        if (!await OwnsParticipantEvent(input.ParticipantId))
            return NotFound();

        if (!ModelState.IsValid)
            return View(input);

        _context.WishlistItems.Add(input);
        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Index),
            new { participantId = input.ParticipantId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w =>
                w.Id == id &&
                w.Participant.Event.CreatorId == CurrentUserId);

        if (item == null)
            return NotFound();

        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,Description,ProductUrl")]
        WishlistItem input)
    {
        if (id != input.Id)
            return BadRequest();

        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w =>
                w.Id == id &&
                w.Participant.Event.CreatorId == CurrentUserId);

        if (item == null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(item);

        item.Title = input.Title;
        item.Description = input.Description;
        item.ProductUrl = input.ProductUrl;

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Index),
            new { participantId = item.ParticipantId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var item = await _context.WishlistItems
            .AsNoTracking()
            .Include(w => w.Participant)
            .FirstOrDefaultAsync(w =>
                w.Id == id &&
                w.Participant.Event.CreatorId == CurrentUserId);

        if (item == null)
            return NotFound();

        return View(item);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int participantId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var participant = await _context.Participants
        .AsNoTracking()
        .Include(p => p.Event)
        .FirstOrDefaultAsync(p => p.Id == participantId && p.Event.CreatorId == userId);

        if (participant == null)
        {
            return NotFound();
        }
        
        ViewBag.Participant = participant;
        ViewBag.ReadOnly = true;

        var items = await _context.WishlistItems
        .AsNoTracking()
        .Where(w => w.ParticipantId == participantId)
        .OrderBy(w => w.Title)
        .ToListAsync();

        return View("Index", items);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w =>
                w.Id == id &&
                w.Participant.Event.CreatorId == CurrentUserId);

        if (item == null)
            return NotFound();

        var participantId = item.ParticipantId;

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Index),
            new { participantId });
    }
}
