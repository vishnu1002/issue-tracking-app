import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TicketModel } from '../../models/ticket.model';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly baseUrl = '/api/tickets';

  constructor(private http: HttpClient) {}

  createTicket(payload: Partial<TicketModel>): Observable<TicketModel> {
    return this.http.post<TicketModel>(this.baseUrl, payload);
  }

  getMyTickets(): Observable<TicketModel[]> {
    return this.http.get<TicketModel[]>(`${this.baseUrl}/me`);
  }

  getAssignedTickets(): Observable<TicketModel[]> {
    return this.http.get<TicketModel[]>(`${this.baseUrl}/assigned`);
  }

  getAllTickets(): Observable<TicketModel[]> {
    return this.http.get<TicketModel[]>(this.baseUrl);
  }

  updateTicket(data: Partial<TicketModel> & { id: number }): Observable<TicketModel> {
    return this.http.put<TicketModel>(`${this.baseUrl}/${data.id}`, data);
  }

  deleteTicket(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
