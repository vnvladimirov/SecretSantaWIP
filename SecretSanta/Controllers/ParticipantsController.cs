using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SecretSanta.Data;
using SecretSanta.Models;
using SQLitePCL;

namespace SecretSanta.Controllers;

public class ParticipantsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ParticipantsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int eventId)
    {
        var exists = await _context
        .Events
        .AnyAsync(e => e.Id == eventId);

        if (!exists)
        {
            return NotFound();
        }

        return View(new Participant { EventId = eventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Email,EventId")] Participant model)
    {
        ModelState.Remove(nameof(Participant.Event));

        var duplicate = await _context.Participants
        .AnyAsync(p => p.EventId == model.EventId && p.Email == model.Email);

        if (duplicate)
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered for this event!");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.Participants.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Events", new { id = model.EventId });
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var participant = await _context.Participants.FindAsync(id);
        if (participant == null)
        {
            return NotFound();
        }
        return View(participant);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,EventId")] Participant model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        ModelState.Remove(nameof(Participant.Event));
        var duplicate = await _context.Participants.AnyAsync(p => p.EventId == model.EventId && p.Email == model.Email && p.Id != model.Id);
        
        if (duplicate)
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered for this event!");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            _context.Update(model);
            await _context.SaveChangesAsync();
        }
        catch(DbUpdateConcurrencyException)
        {
            if (!await _context.Participants.AnyAsync(p => p.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction("Details", "Events", new {id = model.EventId});
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var participant = await _context.Participants.FirstOrDefaultAsync(p => p.Id == id);

        if (participant == null)
        {
            return NotFound();
        }

        return View(participant);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var participant = await _context.Participants.FindAsync(id);

        if (participant == null)
        {
            return RedirectToAction("Index", "Events");
        }

        var eventId = participant.EventId;

        var partners = await _context.Participants
        .Where(p => p.PartnerId == id)
        .ToListAsync();

        foreach (var partner in partners)
        {
            participant.PartnerId = null;
        }

        _context.Participants.Remove(participant);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Events", new { id = eventId });
    }
}