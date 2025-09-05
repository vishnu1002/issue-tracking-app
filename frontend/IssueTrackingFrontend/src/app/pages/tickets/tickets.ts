import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../core/services/ticket.service';
import { TicketModel } from '../../models/ticket.model';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-tickets',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tickets.html',
})
export class Tickets implements OnInit {
  role: string | null = null;
  tickets: TicketModel[] = [];
  loading = signal(false);

  // form for new ticket (user only)
  newTicket: Partial<TicketModel> = {
    title: '',
    description: '',
    priority: 'Low',
    type: 'Software',
  };

  constructor(private ticketService: TicketService, private auth: AuthService) {}

  ngOnInit() {
    this.role = this.auth.getRole(); // ✅ safe now
    this.loadTickets();
  }

  loadTickets() {
    this.loading.set(true);

    if (this.role === 'User') {
      this.ticketService.getMyTickets().subscribe({
        next: (res) => {
          this.tickets = res;
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
    } else if (this.role === 'Representative') {
      this.ticketService.getAssignedTickets().subscribe({
        next: (res) => {
          this.tickets = res;
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
    }
  }

  createTicket() {
    if (this.role !== 'User') return;

    this.ticketService.createTicket(this.newTicket).subscribe({
      next: (ticket) => {
        this.tickets.push(ticket);
        this.newTicket = {
          title: '',
          description: '',
          priority: 'Low',
          type: 'Software',
        };
      },
    });
  }

  updateStatus(ticket: TicketModel, status: TicketModel['status']) {
    if (this.role !== 'Representative') return;

    this.ticketService.updateTicket({ id: ticket.id, status }).subscribe({
      next: (updated) => {
        ticket.status = updated.status;
      },
    });
  }
}
