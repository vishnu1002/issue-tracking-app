import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TicketService } from '../../core/services/ticket.service';
import { TicketModel } from '../../models/ticket.model';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './history.html',
  styleUrl: './history.css',
})
export class History implements OnInit {
  private ticketService = inject(TicketService);
  private auth = inject(AuthService);

  tickets: TicketModel[] = [];
  loading = false;
  error: string | null = null;

  ngOnInit() {
    this.loadHistory();
  }

  loadHistory() {
    this.loading = true;
    this.error = null;

    // Get all tickets and filter for closed ones
    this.ticketService.getAllTickets().subscribe({
      next: (allTickets) => {
        // Filter for closed tickets based on user role
        if (this.auth.getRole() === 'User') {
          // Users see their own closed tickets
          this.tickets = allTickets.filter(
            (ticket) =>
              ticket.status === 'Closed' &&
              ticket.createdByUserId === parseInt(this.auth.getCurrentUser()?.id || '0')
          );
        } else if (this.auth.getRole() === 'Representative') {
          // Representatives see tickets they closed
          this.tickets = allTickets.filter(
            (ticket) =>
              ticket.status === 'Closed' &&
              ticket.assignedToUserId === parseInt(this.auth.getCurrentUser()?.id || '0')
          );
        } else if (this.auth.getRole() === 'Admin') {
          // Admins see all closed tickets
          this.tickets = allTickets.filter((ticket) => ticket.status === 'Closed');
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Failed to load history';
        this.loading = false;
      },
    });
  }
}
