
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Data;
using SecretSanta.Models;

namespace SecretSanta.Controllers;

public class WishlistController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    [HttpGet]
    public async Task<IActionResult> Index(int participantId)
    {
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
        var participantExists = await _context.Participants
            .AnyAsync(p => p.Id == participantId);

        if (!participantExists)
            return NotFound();

        return View(new WishlistItem {ParticipantId = participantId});
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,ProductUrl,ParticipantId")]
        WishlistItem item)
    {
        var participantExists = await _context.Participants
            .AnyAsync(p => p.Id == item.ParticipantId);

        if (!participantExists)
            return NotFound();

        if (!ModelState.IsValid)
            return View(item);

        _context.WishlistItems.Add(item);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { participantId = item.ParticipantId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var item = await _context.WishlistItems.FindAsync(id);

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
            .FirstOrDefaultAsync(w => w.Id == id);

        if (item == null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(item);

        item.Title = input.Title;
        item.Description = input.Description;
        item.ProductUrl = input.ProductUrl;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { participantId = item.ParticipantId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var item = await _context.WishlistItems
            .AsNoTracking()
            .Include(w => w.Participant)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (item == null)
            return NotFound();

        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.WishlistItems.FindAsync(id);

        if (item == null)
            return NotFound();

        var participantId = item.ParticipantId;

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { participantId });
    }
}
