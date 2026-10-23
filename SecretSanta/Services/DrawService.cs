using Microsoft.EntityFrameworkCore;
using SecretSanta.Data;
using SecretSanta.Models;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.Services;

public class DrawService : IDrawService
{
    private readonly ApplicationDbContext _context;

    public DrawService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DrawResult> DrawAsync(int eventId)
    {
        var drawEvent = await _context.Events
        .Include(e => e.Participants)
        .FirstOrDefaultAsync(e => e.Id == eventId);

        if (drawEvent == null)
        {
            return new DrawResult(false, "Event not found :(");
        }

        var previous = await _context.Assignments
        .Where(a => a.EventId == eventId)
        .ToListAsync();

        if (drawEvent.Participants.Count < drawEvent.MinParticipants)
        {
            return new DrawResult(false, $"The event needs at least {drawEvent.MinParticipants} participants!");
        }

        var round = previous.Count == 0 ? 1 : previous.Max(a => a.Round) + 1;

        var forbidden = ForbiddenPairs(drawEvent.Participants, previous);
        var givers = drawEvent.Participants.Select(p => p.Id).ToArray();

        var receivers = FindValidPairing(givers, forbidden);
        if (receivers == null)
        {
            return new DrawResult(false, "No valid draw is possible with the current couples and previous rounds!");
        }

        for (int i = 0; i < givers.Length; i++)
        {
            _context.Assignments.Add(new Assignment
            {
                EventId = eventId,
                Round = round,
                GiverId = givers[i],
                ReceiverId = receivers[i]
            });
        }

        await _context.SaveChangesAsync();
        return new DrawResult(true);

    }


    private static HashSet<(int, int)> ForbiddenPairs(IEnumerable<Participant> participants, IEnumerable<Assignment> previous)
    {
        var forbidden = new HashSet<(int, int)>();

        foreach (var p in participants)
        {
            if (p.PartnerId != null)
            {
                forbidden.Add((p.Id, p.PartnerId.Value));
                forbidden.Add((p.PartnerId.Value, p.Id));
            }
        }

        foreach (var a in previous)
        {
            forbidden.Add((a.GiverId, a.ReceiverId));
        }

        return forbidden;
    }

    private static int[]? FindValidPairing(int[] givers, HashSet<(int, int)> forbidden)
    {
        var receivers = givers.ToArray();

        for (var attempt = 0; attempt < 1000; attempt++)
        {
            Random.Shared.Shuffle(receivers);

            bool isValid = true;
            for (int i = 0; i < givers.Length; i++)
            {
                if (givers[i] == receivers[i] || forbidden.Contains((givers[i], receivers[i])))
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                return receivers;
            }
        }
        return null;
    }
}
