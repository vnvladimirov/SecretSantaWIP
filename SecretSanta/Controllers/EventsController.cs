using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Data;

namespace SecretSanta.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;

    public async Task<IActionResult> Index()
    {
        var events = await _context.Events.ToListAsync();
        return View(events);
    }


    public EventsController(ApplicationDbContext context)
    {
        _context =  context;
    }
}
