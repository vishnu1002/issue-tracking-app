using IssueTrackingAPI.Model;
using IssueTrackingAPI.Repository.TicketRepo.TicketRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IssueTrackingAPI.Controllers;

[ApiController]
[Route("api/attachment")]
[Authorize]
public class AttachmentController : ControllerBase
{
    private readonly ITicketRepo _ticketRepo;
    private readonly string _attachmentsRoot = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "attachments");

    public AttachmentController(ITicketRepo ticketRepo)
    {
        _ticketRepo = ticketRepo;
        if (!Directory.Exists(_attachmentsRoot)) Directory.CreateDirectory(_attachmentsRoot);
    }

    // POST: api/attachment/ticket/{ticketId}
    [HttpPost("ticket/{ticketId}")]
    [RequestSizeLimit(25_000_000)] // ~25MB
    public async Task<IActionResult> Upload(int ticketId, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded" });

        var ticket = await _ticketRepo.GetTicketById(ticketId);
        if (ticket == null) return NotFound(new { message = "Ticket not found" });

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        var canUpload = currentRole switch
        {
            Roles.Admin => true,
            Roles.User => ticket.CreatedByUserId == currentUserId,
            Roles.Rep => ticket.AssignedToUserId == null || ticket.AssignedToUserId == currentUserId,
            _ => false
        };
        if (!canUpload) return Forbid();

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

    // GET: api/attachment/ticket/{ticketId}
    [HttpGet("ticket/{ticketId}")]
    public async Task<IActionResult> List(int ticketId)
    {
        var ticket = await _ticketRepo.GetTicketById(ticketId);
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

    // GET: api/attachment/{attachmentId}
    [HttpGet("{attachmentId}")]
    public async Task<IActionResult> Download(int attachmentId)
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

    // DELETE: api/attachment/{attachmentId}
    [HttpDelete("{attachmentId}")]
    [Authorize(Roles = "Admin")] // Only admins can delete attachments directly
    public async Task<IActionResult> Delete(int attachmentId)
    {
        var deleted = await _ticketRepo.DeleteAttachment(attachmentId);
        if (!deleted) return NotFound(new { message = "Attachment not found" });
        return NoContent();
    }
}


