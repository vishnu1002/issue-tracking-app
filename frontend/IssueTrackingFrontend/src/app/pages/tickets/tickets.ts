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
  filteredTickets: TicketModel[] = [];
  loading = signal(false);

  // Filter properties
  searchTerm = '';
  statusFilter = '';
  priorityFilter = '';
  typeFilter = '';

  constructor(private ticketService: TicketService, private auth: AuthService) {}

  ngOnInit() {
    this.role = this.auth.getRole();
    this.loadTickets();
  }

  loadTickets() {
    this.loading.set(true);

    // The API automatically filters tickets based on user role
    this.ticketService.getAllTickets().subscribe({
      next: (res) => {
        // Sort tickets by creation date (newest first)
        this.tickets = res.sort(
          (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        this.applyFilters();
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  applyFilters() {
    this.filteredTickets = this.tickets.filter((ticket) => {
      const matchesSearch =
        !this.searchTerm ||
        ticket.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        ticket.description.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesStatus =
        !this.statusFilter ||
        ticket.status.toLowerCase() === this.statusFilter.toLowerCase() ||
        this.getStatusDisplay(ticket.status).toLowerCase() === this.statusFilter.toLowerCase();
      const matchesPriority = !this.priorityFilter || ticket.priority === this.priorityFilter;
      const matchesType = !this.typeFilter || ticket.type === this.typeFilter;

      return matchesSearch && matchesStatus && matchesPriority && matchesType;
    });
  }

  clearFilters() {
    this.searchTerm = '';
    this.statusFilter = '';
    this.priorityFilter = '';
    this.typeFilter = '';
    this.applyFilters();
  }

  updateStatus(ticket: TicketModel, status: TicketModel['status']) {
    if (this.role !== 'Rep') return;

    const payload = {
      id: ticket.id,
      title: ticket.title,
      description: ticket.description,
      priority: ticket.priority,
      type: ticket.type,
      status: status,
      assignedToUserId: ticket.assignedToUserId,
      comment: ticket.comment,
    };

    this.ticketService.updateTicket(ticket.id, payload).subscribe({
      next: (updated) => {
        ticket.status = updated.status;
      },
    });
  }

  getStatusDisplay(status: string): string {
    switch (status?.toLowerCase()) {
      case 'open':
        return 'Open';
      case 'in_progress':
      case 'in progress':
        return 'In Progress';
      case 'resolved':
        return 'Resolved';
      case 'closed':
        return 'Closed';
      default:
        return status || 'Unknown';
    }
  }

  deleteTicket(ticket: TicketModel) {
    if (this.role !== 'Admin') return;

    if (
      confirm(
        `Are you sure you want to delete the ticket "${ticket.title}"? This action cannot be undone.`
      )
    ) {
      this.ticketService.deleteTicket(ticket.id).subscribe({
        next: () => {
          // Remove ticket from the list
          this.tickets = this.tickets.filter((t) => t.id !== ticket.id);
          this.applyFilters();
        },
        error: (err) => {
          console.error('Failed to delete ticket:', err);
          alert('Failed to delete ticket. Please try again.');
        },
      });
    }
  }
}
