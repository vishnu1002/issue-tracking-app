using IssueTrackingAPI.Model;

namespace IssueTrackingAPI.Repository.AttachmentRepo.AttachmentRepo;

public interface IAttachmentRepo
{
    Task<IEnumerable<AttachmentModel>> GetAllAttachments();
    Task<AttachmentModel?> GetAttachmentById(int id);
    Task<AttachmentModel> AddAttachment(AttachmentModel attachment);
    Task<bool> DeleteAttachment(int id);
}
