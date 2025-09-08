export interface TicketModel {
  id: number;
  title: string;
  description: string;
  priority: 'Low' | 'Medium' | 'High';
  type: 'Software' | 'Hardware';
  status: string;
  createdByUserId: number;
  createdByUserEmail?: string;
  assignedToUserId?: number;
  comment?: string;
  createdAt: string;
  updatedAt: string;
  attachments?: Array<{
    id: number;
    fileName: string;
    contentType: string;
    fileSizeBytes: number;
    uploadedAt: string;
    uploadedByUserId: number;
  }>;
}
