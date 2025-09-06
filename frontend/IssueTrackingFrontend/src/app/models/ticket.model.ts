import { AttachmentModel } from './attachment.model';

export interface TicketModel {
  id: number;
  title: string;
  description: string;
  priority: 'Low' | 'Medium' | 'High';
  type: 'Software' | 'Hardware';
  status: string;
  createdByUserId: number;
  assignedToUserId?: number;
  comment?: string;
  createdAt: string;
  updatedAt: string;
  attachments?: AttachmentModel[];
}
