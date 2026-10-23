using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Data;
using SecretSanta.Models;
using SecretSanta.Services;
using SQLitePCL;

namespace SecretSanta.Controllers;


public class EventsController(ApplicationDbContext context, IDrawService drawService) : Controller
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDrawService _drawService = drawService;

    public async Task<IActionResult> Index()
    {
        var events = await _context.Events.ToListAsync();
        return View(events);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Name,Description,EventDate,Budget,MinParticipants,OrganizerName,OrganizerEmail")] Event model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _context.Events.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var item = await _context.Events
        .Include(e => e.Participants)
        .Include(e => e.Assignments)
        .FirstOrDefaultAsync(e => e.Id == id);

        if (item == null)
        {
            return NotFound();

        }

        return View(item);
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var item = await _context.Events.FindAsync(id);
        if (item == null)
        {
            return NotFound();

        }
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,Name,Description,EventDate,Budget,MinParticipants,OrganizerName,OrganizerEmail")] Event model)
    {
        if (id != model.Id)
        {
            return NotFound();

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
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Events.AnyAsync(e => e.Id == id))
            {
                return NotFound();

            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var item = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (item == null)
            return NotFound();

        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.Events.FindAsync(id);
        if (item != null)
        {
            _context.Events.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Draw(int id)
    {
        var result = await _drawService.DrawAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "The draw is complete!" : result.Error;

        return RedirectToAction(nameof(Details), new { id });
    }
}
