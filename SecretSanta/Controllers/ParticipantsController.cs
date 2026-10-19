using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SecretSanta.Data;
using SecretSanta.Models;
using SQLitePCL;

namespace SecretSanta.Controllers;

public class ParticipantsController : Controller
{
    private readonly ApplicationDbContext _context;

    private async Task LoadPartnerAsync(Participant participant)
    {
        var candidates = await _context.Participants
        .Where(p => p.EventId == participant.EventId && p.Id != participant.Id && (p.PartnerId == null || p.PartnerId == participant.Id))
        .OrderBy(p => p.Name)
        .ToListAsync();

        ViewBag.Partner = new SelectList(candidates, "Id", "Name", participant.PartnerId);
    }

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
        await LoadPartnerAsync(participant);
        return View(participant);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,EventId,PartnerId")] Participant model)
    {
        var participant = await _context.Participants.FindAsync(id);
        if (participant == null)
        {
            return NotFound();
        }

        ModelState.Remove(nameof(Participant.Event));

        var duplicate = await _context.Participants.AnyAsync(p =>
            p.EventId == participant.EventId && p.Email == model.Email && p.Id != id);
        if (duplicate)
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered for this event!");
        }

        Participant? newPartner = null;
        if (model.PartnerId != null)
        {
            newPartner = await _context.Participants.FindAsync(model.PartnerId);
            if (newPartner == null
                || newPartner.EventId != participant.EventId
                || newPartner.Id == id
                || (newPartner.PartnerId != null && newPartner.PartnerId != id))
            {
                ModelState.AddModelError(nameof(model.PartnerId), "Please choose a valid partner.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.EventId = participant.EventId;
            await LoadPartnerAsync(model);
            return View(model);
        }

        if (participant.PartnerId != null && participant.PartnerId != model.PartnerId)
        {
            var oldPartner = await _context.Participants.FindAsync(participant.PartnerId);
            if (oldPartner != null)
            {
                oldPartner.PartnerId = null;
            }
        }

        participant.Name = model.Name;
        participant.Email = model.Email;
        participant.PartnerId = model.PartnerId;
        if (newPartner != null)
        {
            newPartner.PartnerId = participant.Id;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Events", new { id = participant.EventId });
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