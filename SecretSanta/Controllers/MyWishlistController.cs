
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Data;
using SecretSanta.Models;
using SecretSanta.Security;
using System.Security.Claims;

namespace SecretSanta.Controllers;

[Authorize(AuthenticationSchemes = ParticipantCookieDefaults.AuthenticationScheme)]
public class MyWishlistController(ApplicationDbContext context) : Controller
{
    private readonly ApplicationDbContext _context = context;

    private int? CurrentParticipantId
    {
        get
        {
            var value = User.FindFirstValue(
                ParticipantCookieDefaults.ParticipantIdClaim);

            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public async Task<IActionResult> Index()
    {
        if (CurrentParticipantId is not int participantId)
            return Forbid();

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

        return View("~/Views/Wishlist/Index.cshtml", items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (CurrentParticipantId is not int participantId)
            return Forbid();

        var item = new WishlistItem
        {
            ParticipantId = participantId
        };

        return View("~/Views/Wishlist/Create.cshtml", item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,ProductUrl")] WishlistItem input)
    {
        if (CurrentParticipantId is not int participantId)
            return Forbid();

        var participantExists = await _context.Participants
            .AnyAsync(p => p.Id == participantId);

        if (!participantExists)
            return NotFound();

        if (!ModelState.IsValid)
        {
            input.ParticipantId = participantId;
            return View("~/Views/Wishlist/Create.cshtml", input);
        }

        input.ParticipantId = participantId;

        _context.WishlistItems.Add(input);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (CurrentParticipantId is not int participantId)
            return Forbid();

        if (id == null)
            return NotFound();

        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w =>
                w.Id == id && w.ParticipantId == participantId);

        if (item == null)
            return NotFound();

        return View("~/Views/Wishlist/Edit.cshtml", item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,Description,ProductUrl")] WishlistItem input)
    {
        if (CurrentParticipantId is not int participantId)
            return Forbid();

        if (id != input.Id)
            return BadRequest();

        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w =>
                w.Id == id && w.ParticipantId == participantId);

        if (item == null)
            return NotFound();

        if (!ModelState.IsValid)
            return View("~/Views/Wishlist/Edit.cshtml", item);

        item.Title = input.Title;
        item.Description = input.Description;
        item.ProductUrl = input.ProductUrl;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (CurrentParticipantId is not int participantId)
            return Forbid();

        if (id == null)
            return NotFound();

        var item = await _context.WishlistItems
            .AsNoTracking()
            .FirstOrDefaultAsync(w =>
                w.Id == id && w.ParticipantId == participantId);

        if (item == null)
            return NotFound();

        return View("~/Views/Wishlist/Delete.cshtml", item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (CurrentParticipantId is not int participantId)
            return Forbid();

        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w =>
                w.Id == id && w.ParticipantId == participantId);

        if (item == null)
            return NotFound();

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> RecipientWishlist(int? participantId)
    {
        // Identify the currently authenticated participant and their event.
        var participantIdValue =
            User.FindFirstValue(ParticipantCookieDefaults.ParticipantIdClaim);

        var eventIdValue =
            User.FindFirstValue(ParticipantCookieDefaults.EventIdClaim);

        if (!int.TryParse(participantIdValue, out var currentParticipantId) ||
            !int.TryParse(eventIdValue, out var eventId) ||
            participantId == null)
        {
            return Forbid();
        }

        // Find the most recent draw round for this event.
        var latestRound = await _context.Assignments
            .Where(a => a.EventId == eventId)
            .Select(a => (int?)a.Round)
            .MaxAsync();

        if (latestRound == null)
        {
            return NotFound();
        }

        // Verify that this participant was assigned this exact recipient
        // in the latest round of their own event.
        var isAssignedRecipient = await _context.Assignments
            .AnyAsync(a =>
                a.EventId == eventId &&
                a.Round == latestRound.Value &&
                a.GiverId == currentParticipantId &&
                a.ReceiverId == participantId.Value);

        if (!isAssignedRecipient)
        {
            return NotFound();
        }

        // Load only the verified recipient from the same event.
        var recipient = await _context.Participants
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.Id == participantId.Value &&
                p.EventId == eventId);

        if (recipient == null)
        {
            return NotFound();
        }

        ViewBag.Participant = recipient;
        ViewBag.ReadOnly = true;
        ViewBag.IsRecipientWishlist = true;

        var items = await _context.WishlistItems
            .AsNoTracking()
            .Where(w => w.ParticipantId == recipient.Id)
            .OrderBy(w => w.Title)
            .ToListAsync();

        return View("~/Views/Wishlist/Index.cshtml", items);
    }

}
