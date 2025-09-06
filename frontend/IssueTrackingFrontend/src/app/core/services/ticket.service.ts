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
}
