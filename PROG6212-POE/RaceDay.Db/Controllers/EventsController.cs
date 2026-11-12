using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.DB.Atributes;
using RaceDay.DB.Data;
using RaceDay.DB.Models;
using static RaceDay.DB.Models.Entities;

namespace RaceDay.DB.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>Retrieves all upcoming events.</summary>
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .ToListAsync();
            return Ok(events);
        }

        /// <summary>Retrieves a specific event by ID with categories.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null) return NotFound(new { message = "Event not found." });
            return Ok(ev);
        }

        /// <summary>Creates a new event (Organiser only).</summary>
        [HttpPost]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;

            var ev = new Event
            {
                OrganiserID = organiserId,
                EventName = dto.EventName,
                Description = dto.Description,
                Location = dto.Location,
                EventDate = dto.EventDate
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return StatusCode(201, ev);
        }

        /// <summary>Updates an existing event (Organiser owner only).</summary>
        [HttpPut("{id}")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] CreateEventDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { message = "Event not found." });

            var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;
            if (ev.OrganiserID != organiserId) return Forbid();

            ev.EventName = dto.EventName;
            ev.Description = dto.Description;
            ev.Location = dto.Location;
            ev.EventDate = dto.EventDate;

            await _context.SaveChangesAsync();
            return Ok(ev);
        }

        /// <summary>Deletes an event (Organiser owner only).</summary>
        [HttpDelete("{id}")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { message = "Event not found." });

            var organiserId = HttpContext.Session.GetInt32("UserId")!.Value;
            if (ev.OrganiserID != organiserId) return Forbid();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Event deleted successfully." });
        }
    }

    public record CreateEventDto(string EventName, string? Description, string Location, DateTime EventDate);
}
    

