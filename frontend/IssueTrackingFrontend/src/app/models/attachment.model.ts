export interface AttachmentModel {
  id: number;
  ticketId: number;
  fileName: string;
  fileUrl: string;
  uploadedAt: string; // Date from backend as ISO string
}
