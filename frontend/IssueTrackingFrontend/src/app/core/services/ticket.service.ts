import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TicketModel } from '../../models/ticket.model';
import { env } from '../../../env/env';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly baseUrl = env.apiUrl + '/ticket';

  constructor(private http: HttpClient) {}

  createTicket(payload: {
    title: string;
    description: string;
    priority: string;
    type: string;
    createdByUserId: number;
    assignedToUserId?: number;
    comment?: string;
  }): Observable<TicketModel> {
    return this.http.post<TicketModel>(this.baseUrl, payload);
  }

  getAllTickets(): Observable<TicketModel[]> {
    return this.http.get<TicketModel[]>(this.baseUrl);
  }

  getTicketById(id: number): Observable<TicketModel> {
    return this.http.get<TicketModel>(`${this.baseUrl}/${id}`);
  }

  updateTicket(
    id: number,
    data: {
      id: number;
      title: string;
      description: string;
      priority: string;
      type: string;
      status: string;
      assignedToUserId?: number;
      comment?: string;
    }
  ): Observable<TicketModel> {
    return this.http.put<TicketModel>(`${this.baseUrl}/${id}`, data);
  }

  deleteTicket(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  updateTicketComment(id: number, comment: string): Observable<TicketModel> {
    return this.http.put<TicketModel>(`${this.baseUrl}/${id}/comment`, { comment });
  }

  // Attachments
  uploadAttachment(ticketId: number, file: File): Observable<any> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post(`${this.baseUrl}/${ticketId}/attachments`, form);
  }

  listAttachments(
    ticketId: number
  ): Observable<
    Array<{
      id: number;
      fileName: string;
      contentType: string;
      fileSizeBytes: number;
      uploadedAt: string;
      uploadedByUserId: number;
    }>
  > {
    return this.http.get<
      Array<{
        id: number;
        fileName: string;
        contentType: string;
        fileSizeBytes: number;
        uploadedAt: string;
        uploadedByUserId: number;
      }>
    >(`${this.baseUrl}/${ticketId}/attachments`);
  }

  getAttachmentDownloadUrl(attachmentId: number): string {
    return `${this.baseUrl}/attachment/${attachmentId}`;
  }
}
