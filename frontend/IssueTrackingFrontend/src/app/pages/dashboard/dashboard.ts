import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { TicketService } from '../../core/services/ticket.service';
import { UserService } from '../../core/services/user.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { UserModel } from '../../models/user.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterOutlet, FormsModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'],
})
export class Dashboard implements OnInit {
  auth = inject(AuthService);
  ticketService = inject(TicketService);
  userService = inject(UserService);

  // Wait for the hydrated user signal
  user = toSignal(this.auth.user$, { initialValue: null });

  // Modal state
  showCreateTicketModal = false;
  creatingTicket = false;
  ticketError: string | null = null;
  ticketSuccess: string | null = null;

  // Representatives list
  representatives: UserModel[] = [];

  // New ticket form
  newTicket = {
    title: '',
    description: '',
    priority: 'Low',
    type: 'Software',
    assignedToUserId: null as number | null,
  };

  selectedFile: File | null = null;

  ngOnInit() {
    this.loadRepresentatives();
  }

  loadRepresentatives() {
    this.userService.getRepresentatives().subscribe({
      next: (representatives) => {
        this.representatives = representatives;
      },
      error: (err) => {
        console.error('Error loading representatives:', err);
      },
    });
  }

  openCreateTicketModal() {
    this.showCreateTicketModal = true;
    this.ticketError = null;
    this.ticketSuccess = null;
    this.newTicket = {
      title: '',
      description: '',
      priority: 'Low',
      type: 'Software',
      assignedToUserId: null,
    };
    this.selectedFile = null;
  }

  closeCreateTicketModal() {
    this.showCreateTicketModal = false;
    this.ticketError = null;
    this.ticketSuccess = null;
  }

  onFileSelected(event: any) {
    const f: File | undefined = event?.target?.files?.[0];
    this.selectedFile = f ?? null;
  }

  createTicket() {
    if (!this.newTicket.title || !this.newTicket.description) {
      this.ticketError = 'Please fill in all required fields';
      return;
    }

    this.creatingTicket = true;
    this.ticketError = null;
    this.ticketSuccess = null;

    const currentUser = this.auth.getCurrentUser();
    if (!currentUser) {
      this.ticketError = 'User not found';
      this.creatingTicket = false;
      return;
    }

    const payload = {
      title: this.newTicket.title,
      description: this.newTicket.description,
      priority: this.newTicket.priority,
      type: this.newTicket.type,
      createdByUserId: parseInt(currentUser.id),
      assignedToUserId: this.newTicket.assignedToUserId || undefined,
    };

    this.ticketService.createTicket(payload).subscribe({
      next: (ticket) => {
        if (this.selectedFile) {
          this.ticketService.uploadAttachment(ticket.id, this.selectedFile).subscribe({
            next: () => {
              this.creatingTicket = false;
              this.ticketSuccess = 'Ticket created successfully!';
              setTimeout(() => {
                this.closeCreateTicketModal();
                window.location.reload();
              }, 1200);
            },
            error: () => {
              // Ticket created, but attachment failed
              this.creatingTicket = false;
              this.ticketSuccess = 'Ticket created. Attachment upload failed.';
              setTimeout(() => {
                this.closeCreateTicketModal();
                window.location.reload();
              }, 1500);
            },
          });
        } else {
          this.creatingTicket = false;
          this.ticketSuccess = 'Ticket created successfully!';
          setTimeout(() => {
            this.closeCreateTicketModal();
            window.location.reload();
          }, 1200);
        }
      },
      error: (err) => {
        this.creatingTicket = false;
        this.ticketError = err.error?.message || 'Failed to create ticket. Please try again.';
      },
    });
  }

  // Attachment feature removed
}
