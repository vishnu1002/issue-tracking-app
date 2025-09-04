using IssueTrackingAPI.DTO.AttachmentDTO;
using IssueTrackingAPI.Model;
using IssueTrackingAPI.Repository.AttachmentRepo.AttachmentRepo;
using Microsoft.AspNetCore.Mvc;

namespace IssueTrackingAPI.Controllers;

[ApiController]
[Route("api/attachment")]
public class AttachmentController : ControllerBase
{
    private readonly IAttachmentRepo _attachmentRepo;

    public AttachmentController(IAttachmentRepo attachmentRepo)
    {
        _attachmentRepo = attachmentRepo;
    }

    //
    // Get All Attachments
    // GET: api/attachments/
    //
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
    // Get by Id
    // GET: api/attachments/{id}
    //
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
    // Create Attachment
    // POST: api/attachment/
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
    // Delete Attachment
    // DELETE: api/attachment/{id}
    //
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _attachmentRepo.DeleteAttachment(id);
        if (!deleted) return NotFound(new { message = "Attachment not found" });

        return NoContent();
    }
}
