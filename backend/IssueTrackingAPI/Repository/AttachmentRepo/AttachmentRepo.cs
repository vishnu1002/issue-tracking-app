using IssueTrackingAPI.Context;
using IssueTrackingAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace IssueTrackingAPI.Repository.AttachmentRepo.AttachmentRepo;

public class AttachmentRepo : IAttachmentRepo
{
    private readonly AppDBContext _context;

    public AttachmentRepo(AppDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AttachmentModel>> GetAllAttachments()
    {
        return await _context.Attachments_Table
            .Include(a => a.Ticket)
            .ToListAsync();
    }

    public async Task<AttachmentModel?> GetAttachmentById(int id)
    {
        return await _context.Attachments_Table
            .Include(a => a.Ticket)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<AttachmentModel> AddAttachment(AttachmentModel attachment)
    {
        _context.Attachments_Table.Add(attachment);
        await _context.SaveChangesAsync();
        return attachment;
    }

    public async Task<bool> DeleteAttachment(int id)
    {
        var attachment = await _context.Attachments_Table.FindAsync(id);
        if (attachment == null) return false;

        _context.Attachments_Table.Remove(attachment);
        await _context.SaveChangesAsync();
        return true;
    }
}
