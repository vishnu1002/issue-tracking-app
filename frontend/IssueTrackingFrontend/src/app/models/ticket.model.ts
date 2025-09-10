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
  assignedToUserEmail?: string;
  comment?: string;
  createdAt: string;
  updatedAt: string;
  resolvedAt?: string;
  resolutionTime?: string; // TimeSpan serialized as string (e.g., HH:mm:ss)
  resolutionNotes?: string;
  attachments?: Array<{
    id: number;
    fileName: string;
    contentType: string;
    fileSizeBytes: number;
    uploadedAt: string;
    uploadedByUserId: number;
  }>;
}
