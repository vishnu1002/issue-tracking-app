using IssueTrackingAPI.DTO.TicketDTO;
using IssueTrackingAPI.Model;
using IssueTrackingAPI.Repository.TicketRepo.TicketRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace IssueTrackingAPI.Controllers;

[ApiController]
[Route("api/ticket")]
[Authorize]
public class TicketController : ControllerBase
{
    private readonly ITicketRepo _ticketRepo;

    public TicketController(ITicketRepo ticketRepo)
    {
        _ticketRepo = ticketRepo;
    }

    //
    // Get All Tickets
    // GET: api/tickets
    //
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketRead_DTO>>> GetAllTickets()
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        IEnumerable<TicketModel> tickets;

        switch (currentRole)
        {
            case Roles.Admin:
                tickets = await _ticketRepo.GetAllTickets();
                break;
            case Roles.User:
                tickets = await _ticketRepo.GetTicketsByCreator(currentUserId); // only tickets created by user
                break;
            case Roles.Rep:
                tickets = await _ticketRepo.GetTicketsByAssignee(currentUserId); // only tickets assigned to rep
                break;
            default:
                return Forbid();
        }

        var ticketDtos = tickets.Select(t => new TicketRead_DTO
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Priority = t.Priority,
            Type = t.Type,
            Status = t.Status,
            CreatedByUserId = t.CreatedByUserId,
            AssignedToUserId = t.AssignedToUserId,
            Comment = t.Comment,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        });

        return Ok(ticketDtos);
    }

    //
    // Get Tickets By Id
    // GET: api/tickets/{id}
    //
    [HttpGet("{id}")]
    public async Task<ActionResult<TicketRead_DTO>> GetTicketById(int id)
    {
        var ticket = await _ticketRepo.GetTicketById(id);
        if (ticket == null) return NotFound(new { message = "Ticket not found" });

        var ticketDto = new TicketRead_DTO
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Priority = ticket.Priority,
            Type = ticket.Type,
            Status = ticket.Status,
            CreatedByUserId = ticket.CreatedByUserId,
            AssignedToUserId = ticket.AssignedToUserId,
            Comment = ticket.Comment,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };

        return Ok(ticketDto);
    }

    //
    // Create Ticket
    // POST: api/tickets
    // 
    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public async Task<ActionResult<TicketRead_DTO>> CreateTicket([FromBody] TicketCreate_DTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var ticket = new TicketModel
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Type = dto.Type,
            Status = "Open",
            CreatedByUserId = dto.CreatedByUserId,
            AssignedToUserId = dto.AssignedToUserId,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdTicket = await _ticketRepo.AddTicket(ticket);

        var ticketDto = new TicketRead_DTO
        {
            Id = createdTicket.Id,
            Title = createdTicket.Title,
            Description = createdTicket.Description,
            Priority = createdTicket.Priority,
            Type = createdTicket.Type,
            Status = createdTicket.Status,
            CreatedByUserId = createdTicket.CreatedByUserId,
            AssignedToUserId = createdTicket.AssignedToUserId,
            Comment = createdTicket.Comment,
            CreatedAt = createdTicket.CreatedAt,
            UpdatedAt = createdTicket.UpdatedAt
        };

        return CreatedAtAction(nameof(GetTicketById), new { id = ticketDto.Id }, ticketDto);
    }

    //
    // Update Ticker
    // PUT: api/tickets/{id}
    //
    [HttpPut("{id}")]
    public async Task<ActionResult<TicketRead_DTO>> UpdateTicket(int id, [FromBody] TicketUpdate_DTO dto)
    {
        var currentUserId = int.Parse(User.FindFirst("id").Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        if (id != dto.Id) return BadRequest(new { message = "Ticket ID mismatch" });

        var existing = await _ticketRepo.GetTicketById(id);
        if (existing == null) return NotFound(new { message = "Ticket not found" });

        // Role restrictions
        if (currentRole == "User" && existing.CreatedByUserId != currentUserId)
            return Forbid();
        if (currentRole == "Rep" && existing.AssignedToUserId != currentUserId)
            return Forbid();

        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.Priority = dto.Priority;
        existing.Type = dto.Type;
        existing.Status = dto.Status;
        existing.AssignedToUserId = dto.AssignedToUserId;
        existing.Comment = dto.Comment;

        var updatedTicket = await _ticketRepo.UpdateTicket(existing);

        return Ok(new TicketRead_DTO
        {
            Id = updatedTicket.Id,
            Title = updatedTicket.Title,
            Description = updatedTicket.Description,
            Priority = updatedTicket.Priority,
            Type = updatedTicket.Type,
            Status = updatedTicket.Status,
            CreatedByUserId = updatedTicket.CreatedByUserId,
            AssignedToUserId = updatedTicket.AssignedToUserId,
            Comment = updatedTicket.Comment,
            CreatedAt = updatedTicket.CreatedAt,
            UpdatedAt = updatedTicket.UpdatedAt
        });
    }

    //
    // Delete Ticket
    // DELETE: api/tickets/{id}
    //
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var deleted = await _ticketRepo.DeleteTicket(id);
        if (!deleted) return NotFound(new { message = "Ticket not found" });

        return NoContent();
    }
}
