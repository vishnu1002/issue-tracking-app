using Microsoft.EntityFrameworkCore;
using IssueTrackingAPI.DTO.TicketDTO;
using IssueTrackingAPI.Model;
using IssueTrackingAPI.Repository.TicketRepo.TicketRepo;
// using IssueTrackingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using Microsoft.Net.Http.Headers;

namespace IssueTrackingAPI.Controllers;

[ApiController]
[Route("api/ticket")]
[Authorize]
public class TicketController : ControllerBase
{
    private readonly ITicketRepo _ticketRepo;
    private readonly string _attachmentsRoot = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "attachments");

    public TicketController(ITicketRepo ticketRepo)
    {
        _ticketRepo = ticketRepo;
        if (!Directory.Exists(_attachmentsRoot)) Directory.CreateDirectory(_attachmentsRoot);
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
            CreatedByUserEmail = t.CreatedByUser?.Email,
            AssignedToUserId = t.AssignedToUserId,
            AssignedToUserEmail = t.AssignedToUser?.Email,
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
            CreatedByUserEmail = ticket.CreatedByUser?.Email,
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToUserEmail = ticket.AssignedToUser?.Email,
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
            CreatedAt = IssueTrackingAPI.Context.TimeHelper.NowIst(),
            UpdatedAt = IssueTrackingAPI.Context.TimeHelper.NowIst()
        };

        var createdTicket = await _ticketRepo.AddTicket(ticket);

        // Notifications removed

        var ticketDto = new TicketRead_DTO
        {
            Id = createdTicket.Id,
            Title = createdTicket.Title,
            Description = createdTicket.Description,
            Priority = createdTicket.Priority,
            Type = createdTicket.Type,
            Status = createdTicket.Status,
            CreatedByUserId = createdTicket.CreatedByUserId,
            CreatedByUserEmail = createdTicket.CreatedByUser?.Email,
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

        // Preserve existing fields if client omits them (legacy tickets may have nulls on client)
        existing.Title = string.IsNullOrWhiteSpace(dto.Title) ? existing.Title : dto.Title;
        existing.Description = string.IsNullOrWhiteSpace(dto.Description) ? existing.Description : dto.Description;
        existing.Priority = string.IsNullOrWhiteSpace(dto.Priority) ? existing.Priority : dto.Priority;
        existing.Type = string.IsNullOrWhiteSpace(dto.Type) ? existing.Type : dto.Type;
        existing.Status = string.IsNullOrWhiteSpace(dto.Status) ? existing.Status : dto.Status;
        // Auto-assign on first Rep touch so other reps won't see this ticket anymore
        if (currentRole == Roles.Rep)
        {
            if (existing.AssignedToUserId == null)
            {
                existing.AssignedToUserId = currentUserId;
            }
            else
            {
                existing.AssignedToUserId = dto.AssignedToUserId;
            }
        }
        else
        {
            existing.AssignedToUserId = dto.AssignedToUserId;
        }
        existing.Comment = dto.Comment;
        existing.ResolutionNotes = dto.ResolutionNotes;

        try
        {
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
                CreatedByUserEmail = updatedTicket.CreatedByUser?.Email,
                AssignedToUserId = updatedTicket.AssignedToUserId,
                AssignedToUserEmail = updatedTicket.AssignedToUser?.Email,
                Comment = updatedTicket.Comment,
                CreatedAt = updatedTicket.CreatedAt,
                UpdatedAt = updatedTicket.UpdatedAt
            });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new
            {
                status = 500,
                message = "Internal Server Error. Please try again later.",
                detail = ex.InnerException?.Message ?? ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = 500,
                message = "Internal Server Error. Please try again later.",
                detail = ex.InnerException?.Message ?? ex.Message
            });
        }
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

        // Auto-assign on first Rep touch via comment as well
        if (currentRole == Roles.Rep && existing.AssignedToUserId == null)
        {
            existing.AssignedToUserId = currentUserId;
        }

        existing.Comment = request.Comment;
        existing.UpdatedAt = IssueTrackingAPI.Context.TimeHelper.NowIst();

        var updatedTicket = await _ticketRepo.UpdateTicket(existing);

        // Notifications removed

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
            AssignedToUserEmail = updatedTicket.AssignedToUser?.Email,
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

    //
    // Upload Attachment
    // POST: api/ticket/{ticketId}/attachments
    //
    [HttpPost("{ticketId}/attachments")]
    [RequestSizeLimit(25_000_000)] // ~25MB
    public async Task<IActionResult> UploadAttachment(int ticketId, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded" });

        var ticket = await _ticketRepo.GetTicketById(ticketId);
        if (ticket == null) return NotFound(new { message = "Ticket not found" });

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        // Only Admin or ticket creator can upload attachments; Reps can upload for tickets assigned to them
        var canUpload = currentRole switch
        {
            Roles.Admin => true,
            Roles.User => ticket.CreatedByUserId == currentUserId,
            Roles.Rep => ticket.AssignedToUserId == null || ticket.AssignedToUserId == currentUserId,
            _ => false
        };
        if (!canUpload) return Forbid();

        // Validate content type (images, pdf, txt)
        var allowed = new[] { "image/png", "image/jpeg", "image/gif", "application/pdf", "text/plain" };
        if (!allowed.Contains(file.ContentType))
            return BadRequest(new { message = "Unsupported file type" });

        var storedName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
        var savePath = Path.Combine(_attachmentsRoot, storedName);
        using (var stream = new FileStream(savePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new AttachmentModel
        {
            FileName = file.FileName,
            StoredFileName = storedName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            FilePath = savePath,
            TicketId = ticketId,
            UploadedByUserId = currentUserId,
            UploadedAt = IssueTrackingAPI.Context.TimeHelper.NowIst()
        };

        await _ticketRepo.AddAttachment(attachment);
        return Ok(new { id = attachment.Id, message = "File uploaded" });
    }

    //
    // List Attachments
    // GET: api/ticket/{ticketId}/attachments
    //
    [HttpGet("{ticketId}/attachments")]
    public async Task<IActionResult> ListAttachments(int ticketId)
    {
        var ticket = await _ticketRepo.GetTicketById(ticketId);
        if (ticket == null) return NotFound(new { message = "Ticket not found" });

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        // Access: Admin, creator, or Rep assigned/unassigned
        var canView = currentRole switch
        {
            Roles.Admin => true,
            Roles.User => ticket.CreatedByUserId == currentUserId,
            Roles.Rep => ticket.AssignedToUserId == null || ticket.AssignedToUserId == currentUserId,
            _ => false
        };
        if (!canView) return Forbid();

        var items = await _ticketRepo.GetAttachmentsByTicket(ticketId);
        return Ok(items.Select(a => new
        {
            a.Id,
            a.FileName,
            a.ContentType,
            a.FileSizeBytes,
            a.UploadedAt,
            a.UploadedByUserId
        }));
    }

    //
    // Download Attachment
    // GET: api/ticket/attachment/{attachmentId}
    //
    [HttpGet("attachment/{attachmentId}")]
    public async Task<IActionResult> DownloadAttachment(int attachmentId)
    {
        var attachment = await _ticketRepo.GetAttachmentById(attachmentId);
        if (attachment == null) return NotFound(new { message = "Attachment not found" });

        var ticket = await _ticketRepo.GetTicketById(attachment.TicketId);
        if (ticket == null) return NotFound(new { message = "Ticket not found" });

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        var canView = currentRole switch
        {
            Roles.Admin => true,
            Roles.User => ticket.CreatedByUserId == currentUserId,
            Roles.Rep => ticket.AssignedToUserId == null || ticket.AssignedToUserId == currentUserId,
            _ => false
        };
        if (!canView) return Forbid();

        if (!System.IO.File.Exists(attachment.FilePath))
            return NotFound(new { message = "File missing on server" });

        var fileBytes = await System.IO.File.ReadAllBytesAsync(attachment.FilePath);
        return File(fileBytes, attachment.ContentType, attachment.FileName);
    }
}

// Request model for comment updates
public class UpdateCommentRequest
{
    public string? Comment { get; set; }
}
