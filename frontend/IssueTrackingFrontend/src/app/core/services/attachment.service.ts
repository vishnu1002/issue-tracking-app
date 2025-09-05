import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AttachmentModel } from '../../models/attachment.model';

@Injectable({ providedIn: 'root' })
export class AttachmentService {
  private readonly baseUrl = '/api/attachments';

  constructor(private http: HttpClient) {}

  // Fetch all attachments for a ticket
  getAttachments(ticketId: number): Observable<AttachmentModel[]> {
    return this.http.get<AttachmentModel[]>(`${this.baseUrl}/ticket/${ticketId}`);
  }

  // Upload a new file to a ticket
  uploadAttachment(ticketId: number, file: File): Observable<AttachmentModel> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<AttachmentModel>(`${this.baseUrl}/${ticketId}`, formData);
  }

  // Delete an attachment by ID
  deleteAttachment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
