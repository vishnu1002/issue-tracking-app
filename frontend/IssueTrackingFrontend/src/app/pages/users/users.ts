import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../core/services/user.service';
import { TicketService } from '../../core/services/ticket.service';
import { ToastService } from '../../core/services/toast.service';
import { UserModel } from '../../models/user.model';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './users.html',
  styleUrl: './users.css',
})
export class Users implements OnInit {
  private userService = inject(UserService);
  private ticketService = inject(TicketService);
  private toast = inject(ToastService);

  users: UserModel[] = [];
  filteredUsers: UserModel[] = [];
  loading = false;
  error: string | null = null;

  // Map of userId -> created ticket count
  ticketCounts: Record<string, number> = {};

  // Search properties
  searchTerm = '';
  roleFilter = '';

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.loading = true;
    this.error = null;

    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.users = users;
        // As Admin, fetch all tickets to compute counts per creator
        this.ticketService.getAllTickets().subscribe({
          next: (tickets) => {
            const counts: Record<string, number> = {};
            for (const t of tickets) {
              const key = String(t.createdByUserId);
              counts[key] = (counts[key] || 0) + 1;
            }
            this.ticketCounts = counts;
            this.applyFilters();
            this.loading = false;
          },
          error: () => {
            // If ticket loading fails, proceed without counts
            this.ticketCounts = {};
            this.applyFilters();
            this.loading = false;
          },
        });
      },
      error: (err) => {
        this.error = err.error?.message || 'Failed to load users';
        this.loading = false;
      },
    });
  }

  applyFilters() {
    this.filteredUsers = this.users.filter((user) => {
      const matchesSearch =
        !this.searchTerm ||
        user.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesRole = !this.roleFilter || user.role === this.roleFilter;

      return matchesSearch && matchesRole;
    });
  }

  clearFilters() {
    this.searchTerm = '';
    this.roleFilter = '';
    this.applyFilters();
  }

  deleteUser(user: UserModel) {
    if (
      confirm(`Are you sure you want to delete user "${user.name}"? This action cannot be undone.`)
    ) {
      this.userService.deleteUser(user.id.toString()).subscribe({
        next: () => {
          // Remove user from the list
          this.users = this.users.filter((u) => u.id !== user.id);
          this.applyFilters();
          this.toast.success('User deleted');
        },
        error: (err) => {
          console.error('Failed to delete user:', err);

          // Handle specific error messages from the backend
          if (err.status === 400 && err.error?.message) {
            this.toast.error(err.error.message);
          } else if (err.status === 404) {
            this.toast.error('User not found.');
          } else {
            this.toast.error('Failed to delete user. Please try again.');
          }
        },
      });
    }
  }
}
