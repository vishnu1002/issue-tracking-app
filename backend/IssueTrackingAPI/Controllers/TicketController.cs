using IssueTrackingAPI.DTO.TicketDTO;
using IssueTrackingAPI.Model;
using IssueTrackingAPI.Repository.TicketRepo.TicketRepo;
using IssueTrackingAPI.Services;
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
    private readonly INotificationSignalRService _notificationService;

    public TicketController(ITicketRepo ticketRepo, INotificationSignalRService notificationService)
    {
        _ticketRepo = ticketRepo;
        _notificationService = notificationService;
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
                // Representatives can see tickets assigned to them AND unassigned tickets
                var assignedTickets = await _ticketRepo.GetTicketsByAssignee(currentUserId);
                var unassignedTickets = await _ticketRepo.GetUnassignedTickets();
                tickets = assignedTickets.Concat(unassignedTickets);
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
            UpdatedAt = t.UpdatedAt,
            ResolvedAt = t.ResolvedAt,
            ResolutionTime = t.ResolutionTime,
            ResolutionNotes = t.ResolutionNotes
        });

        return Ok(ticketDtos);
    }

    //
    // Search Tickets
    // GET: api/ticket/search
    //
    [HttpGet("search")]
    public async Task<ActionResult<object>> SearchTickets([FromQuery] TicketSearch_DTO searchDto)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        // Apply role-based filtering to search
        switch (currentRole)
        {
            case Roles.Admin:
                // Admins can search all tickets
                break;
            case Roles.User:
                // Users can only search their own tickets
                searchDto.CreatedByUserId = currentUserId;
                break;
            case Roles.Rep:
                // Representatives can search tickets assigned to them and unassigned tickets
                // We'll handle this in the repository by modifying the search logic
                break;
            default:
                return Forbid();
        }

        var (tickets, totalCount) = await _ticketRepo.SearchTickets(searchDto);

        // For Representatives, filter to show only assigned and unassigned tickets
        if (currentRole == Roles.Rep)
        {
            tickets = tickets.Where(t => t.AssignedToUserId == currentUserId || t.AssignedToUserId == null);
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
            UpdatedAt = t.UpdatedAt,
            ResolvedAt = t.ResolvedAt,
            ResolutionTime = t.ResolutionTime,
            ResolutionNotes = t.ResolutionNotes
        });

        return Ok(new
        {
            tickets = ticketDtos,
            totalCount = totalCount,
            pageNumber = searchDto.PageNumber,
            pageSize = searchDto.PageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
        });
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
            Comment = dto.Comment ?? string.Empty,
            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
            UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
        };

        var createdTicket = await _ticketRepo.AddTicket(ticket);

        // Send notifications (with error handling)
        try
        {
            await _notificationService.SendNotificationToAdminsAsync(
                createdTicket.Id, 
                "TicketCreated", 
                $"New ticket '{createdTicket.Title}' has been created");

            if (createdTicket.AssignedToUserId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    createdTicket.AssignedToUserId.Value,
                    createdTicket.Id,
                    "TicketAssigned",
                    $"You have been assigned to ticket '{createdTicket.Title}'");
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the ticket creation
            // In a production app, you'd want to use proper logging
            Console.WriteLine($"Notification error: {ex.Message}");
        }

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
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        if (id != dto.Id) return BadRequest(new { message = "Ticket ID mismatch" });

        var existing = await _ticketRepo.GetTicketById(id);
        if (existing == null) return NotFound(new { message = "Ticket not found" });

        // Role restrictions
        if (currentRole == "User" && existing.CreatedByUserId != currentUserId)
            return Forbid();
        if (currentRole == "Rep" && existing.AssignedToUserId != null && existing.AssignedToUserId != currentUserId)
            return Forbid();

        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.Priority = dto.Priority;
        existing.Type = dto.Type;
        existing.Status = dto.Status;
        existing.AssignedToUserId = dto.AssignedToUserId;
        existing.Comment = dto.Comment;
        existing.ResolutionNotes = dto.ResolutionNotes;

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
    // Update Ticket Comment
    // PUT: api/ticket/{id}/comment
    //
    [HttpPut("{id}/comment")]
    [Authorize(Roles = "Rep,Admin")]
    public async Task<ActionResult<TicketRead_DTO>> UpdateTicketComment(int id, [FromBody] UpdateCommentRequest request)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        var existing = await _ticketRepo.GetTicketById(id);
        if (existing == null) return NotFound(new { message = "Ticket not found" });

        // Role restrictions - Reps can comment on tickets assigned to them OR unassigned tickets
        if (currentRole == "Rep" && existing.AssignedToUserId != null && existing.AssignedToUserId != currentUserId)
            return StatusCode(403, new { message = "You can only comment on tickets assigned to you or unassigned tickets" });

        existing.Comment = request.Comment;
        existing.UpdatedAt = DateTime.UtcNow;

        var updatedTicket = await _ticketRepo.UpdateTicket(existing);

        // Send notification to ticket creator about the comment
        try
        {
            if (existing.CreatedByUserId != currentUserId)
            {
                await _notificationService.SendNotificationAsync(
                    existing.CreatedByUserId,
                    existing.Id,
                    "TicketCommented",
                    $"A comment has been added to your ticket '{existing.Title}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Notification error: {ex.Message}");
        }

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

// Request model for comment updates
public class UpdateCommentRequest
{
    public string? Comment { get; set; }
}
