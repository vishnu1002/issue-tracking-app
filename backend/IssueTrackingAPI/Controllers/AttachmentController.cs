using IssueTrackingAPI.DTO.AttachmentDTO;
using IssueTrackingAPI.Model;
using IssueTrackingAPI.Repository.AttachmentRepo.AttachmentRepo;
using IssueTrackingAPI.Repository.TicketRepo.TicketRepo; // ✅ Import Ticket Repo
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IssueTrackingAPI.Controllers;

[ApiController]
[Route("api/attachment")]
[Authorize] // Requires authorize for all calls
public class AttachmentController : ControllerBase
{
    private readonly IAttachmentRepo _attachmentRepo;
    private readonly ITicketRepo _ticketRepo;

    public AttachmentController(IAttachmentRepo attachmentRepo, ITicketRepo ticketRepo)
    {
        _attachmentRepo = attachmentRepo;
        _ticketRepo = ticketRepo;
    }

    // 
    // Get All Tickets
    // GET: /api/attachment
    //
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttachmentRead_DTO>>> GetAll()
    {
        var attachments = await _attachmentRepo.GetAllAttachments();
        var dtoList = attachments.Select(a => new AttachmentRead_DTO
        {
            Id = a.Id,
            TicketId = a.TicketId,
            FileName = a.FileName,
            FileUrl = a.FileUrl,
            UploadedAt = a.UploadedAt
        });
        return Ok(dtoList);
    }

    //
    // Get Attachment By Id
    // GET: /api/attachment/api
    //
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<AttachmentRead_DTO>> GetById(int id)
    {
        var attachment = await _attachmentRepo.GetAttachmentById(id);
        if (attachment == null) return NotFound(new { message = "Attachment not found" });

        return Ok(new AttachmentRead_DTO
        {
            Id = attachment.Id,
            TicketId = attachment.TicketId,
            FileName = attachment.FileName,
            FileUrl = attachment.FileUrl,
            UploadedAt = attachment.UploadedAt
        });
    }

    //
    // Create Ticket
    // PUT: /api/attachment/{id}
    //
    [HttpPost]
    public async Task<ActionResult<AttachmentRead_DTO>> Create([FromBody] AttachmentCreate_DTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var attachment = new AttachmentModel
        {
            TicketId = dto.TicketId,
            FileName = dto.FileName,
            FileUrl = dto.FileUrl
        };

        var created = await _attachmentRepo.AddAttachment(attachment);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new AttachmentRead_DTO
        {
            Id = created.Id,
            TicketId = created.TicketId,
            FileName = created.FileName,
            FileUrl = created.FileUrl,
            UploadedAt = created.UploadedAt
        });
    }

    //
    // Delete Ticket
    // DELETE: /api/attachment/{id}
    //
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        var attachment = await _attachmentRepo.GetAttachmentById(id);
        if (attachment == null) return NotFound(new { message = "Attachment not found" });

        if (currentRole != "Admin")
        {
            var ticket = await _ticketRepo.GetTicketById(attachment.TicketId);
            if (ticket == null || ticket.CreatedByUserId != currentUserId)
                return Forbid();
        }

        var deleted = await _attachmentRepo.DeleteAttachment(id);
        if (!deleted) return NotFound(new { message = "Attachment not found" });

        return NoContent();
    }
}
