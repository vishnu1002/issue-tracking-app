import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { TicketService } from '../../core/services/ticket.service';
import { TicketModel } from '../../models/ticket.model';
import { AuthService } from '../../core/services/auth.service';
import { ActivatedRoute } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';

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
  // Admin-only: filter by Rep email via searchTerm as well

  constructor(
    private ticketService: TicketService,
    private auth: AuthService,
    private toast: ToastService,
    private route: ActivatedRoute,
    private title: Title
  ) {}

  ngOnInit() {
    this.title.setTitle('Issue Tracker - Tickets');
    this.role = this.auth.getRole();
    this.loadTickets();
    // Show toast when redirected after creating a ticket (via query param or navigation state)
    const createdParam = this.route.snapshot.queryParamMap.get('created');
    const navState = (history.state && (history.state as any).ticketCreated) || false;
    if (createdParam === '1' || createdParam === 'true' || navState) {
      this.toast.success('Ticket created successfully');
    }
  }

  getAttachmentUrl(id: number): string {
    return this.ticketService.getAttachmentDownloadUrl(id);
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
        // After tickets arrive, fetch attachments for each ticket for Rep view
        this.tickets.forEach((t) => {
          this.ticketService.listAttachments(t.id).subscribe({
            next: (atts) => (t.attachments = atts),
          });
        });
        this.applyFilters();
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  // Inline ticket creation moved to Dashboard modal

  applyFilters() {
    this.filteredTickets = this.tickets.filter((ticket) => {
      const q = this.searchTerm.trim().toLowerCase();
      const matchesSearch =
        !q ||
        String(ticket.id) === q.replace(/^#/, '') ||
        ticket.title.toLowerCase().includes(q) ||
        ticket.description.toLowerCase().includes(q) ||
        (this.role === 'Admin' && (ticket.createdByUserEmail || '').toLowerCase().includes(q)) ||
        (this.role === 'Admin' && (ticket.assignedToUserEmail || '').toLowerCase().includes(q));

      const statusVal = this.getStatusDisplay(ticket.status).toLowerCase();
      const filterVal = (this.statusFilter || '').toLowerCase().replace('_', ' ');
      const matchesStatus =
        !filterVal || statusVal === filterVal || ticket.status.toLowerCase() === filterVal;
      const matchesPriority = !this.priorityFilter || ticket.priority === this.priorityFilter;
      const matchesType = !this.typeFilter || ticket.type === this.typeFilter;

      return matchesSearch && matchesStatus && matchesPriority && matchesType;
    });
  }

  // Admin: compute time taken text for closed tickets
  getClosedTimeTaken(ticket: TicketModel): string | null {
    const status = (ticket.status || '').toLowerCase();
    if (status !== 'closed') return null;
    // Prefer computing from createdAt -> resolvedAt/updatedAt
    const startMs = new Date(ticket.createdAt).getTime();
    const endMs = new Date(ticket.resolvedAt || ticket.updatedAt).getTime();
    if (isFinite(startMs) && isFinite(endMs) && endMs >= startMs) {
      const diffMs = endMs - startMs;
      const totalHours = diffMs / 3600000;
      if (totalHours < 1) {
        const minutes = Math.round(diffMs / 60000);
        return `Time taken: ${minutes} minutes`;
      }
      const hoursRounded = Math.round(totalHours);
      return `Time taken: ${hoursRounded} hours`;
    }
    // Fallback: parse resolutionTime when timestamps are not usable
    if (ticket.resolutionTime) {
      const span = ticket.resolutionTime.trim();
      let days = 0,
        hours = 0,
        minutes = 0,
        seconds = 0;
      if (span.includes('.')) {
        const [d, rest] = span.split('.');
        days = parseInt(d, 10) || 0;
        const parts = rest.split(':');
        hours = parseInt(parts[0], 10) || 0;
        minutes = parseInt(parts[1], 10) || 0;
        seconds = parseInt(parts[2] || '0', 10) || 0;
      } else {
        const parts = span.split(':');
        hours = parseInt(parts[0], 10) || 0;
        minutes = parseInt(parts[1], 10) || 0;
        seconds = parseInt(parts[2] || '0', 10) || 0;
      }
      const totalHours = days * 24 + hours + minutes / 60 + seconds / 3600;
      if (totalHours < 1) {
        const totalMinutes = Math.round(totalHours * 60);
        return `Time taken: ${totalMinutes} minutes`;
      }
      const hoursRounded = Math.round(totalHours);
      return `Time taken: ${hoursRounded} hours`;
    }
    return null;
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

    // Check if trying to change from Closed to In Progress
    if (
      (ticket.status === 'CLOSED' || ticket.status === 'Closed') &&
      (status === 'IN_PROGRESS' || status === 'In Progress')
    ) {
      const confirmed = confirm('Do you want to reopen the ticket?');
      if (!confirmed) {
        return; // Keep it closed
      }
    }

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
        this.toast.success(`Status updated to ${this.getStatusDisplay(updated.status)}`);
      },
    });
  }

  // Comment functionality for Reps
  editingComment: { [key: number]: boolean } = {};
  commentText: { [key: number]: string } = {};

  startEditingComment(ticket: TicketModel) {
    if (this.role !== 'Rep') return;
    this.editingComment[ticket.id] = true;
    this.commentText[ticket.id] = ticket.comment || '';
  }

  cancelEditingComment(ticket: TicketModel) {
    this.editingComment[ticket.id] = false;
    this.commentText[ticket.id] = '';
  }

  saveComment(ticket: TicketModel) {
    if (this.role !== 'Rep') return;

    this.ticketService.updateTicketComment(ticket.id, this.commentText[ticket.id]).subscribe({
      next: (updated) => {
        ticket.comment = updated.comment;
        this.editingComment[ticket.id] = false;
        this.commentText[ticket.id] = '';
      },
      error: (err) => {
        console.error('Failed to save comment:', err);
        alert('Failed to save comment. Please try again.');
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
          this.toast.success('Ticket deleted');
        },
        error: (err) => {
          console.error('Failed to delete ticket:', err);
          this.toast.error('Failed to delete ticket. Please try again.');
        },
      });
    }
  }
}
