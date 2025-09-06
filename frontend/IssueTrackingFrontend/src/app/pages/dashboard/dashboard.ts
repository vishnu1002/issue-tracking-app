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
    this.selectedFile = event.target.files[0];
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
        this.creatingTicket = false;
        this.ticketSuccess = 'Ticket created successfully!';

        // If file is selected, create attachment record
        if (this.selectedFile) {
          this.createAttachmentRecord(ticket.id);
        }

        setTimeout(() => {
          this.closeCreateTicketModal();
          // Refresh the tickets page if we're on it
          window.location.reload();
        }, 1500);
      },
      error: (err) => {
        this.creatingTicket = false;
        this.ticketError = err.error?.message || 'Failed to create ticket. Please try again.';
      },
    });
  }

  private createAttachmentRecord(ticketId: number) {
    if (!this.selectedFile) return;

    // Create a random file path (as requested, not storing actual file)
    const randomPath = `attachments/${Date.now()}_${Math.random().toString(36).substring(7)}_${
      this.selectedFile.name
    }`;

    const attachmentData = {
      ticketId: ticketId,
      fileName: this.selectedFile.name,
      filePath: randomPath,
      fileSize: this.selectedFile.size,
      uploadedByUserId: parseInt(this.auth.getCurrentUser()?.id || '0'),
    };

    // Note: This would need an attachment service to be implemented
    // For now, we'll just log it
    console.log('Attachment data:', attachmentData);
  }
}
