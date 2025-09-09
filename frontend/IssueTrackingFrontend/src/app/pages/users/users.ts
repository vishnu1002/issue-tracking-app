import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Title } from '@angular/platform-browser';
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
  private title = inject(Title);

  users: UserModel[] = [];
  filteredUsers: UserModel[] = [];
  loading = false;
  error: string | null = null;

  // Map of userId -> created ticket count
  ticketCounts: Record<string, number> = {};

  // Search properties
  searchTerm = '';
  roleFilter = '';

  // Role popover state (by user id)
  rolePopoverForId: string | null = null;

  ngOnInit() {
    this.title.setTitle('Issue Tracker - Users');
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

  toggleRolePopover(userId: string) {
    this.rolePopoverForId = this.rolePopoverForId === userId ? null : userId;
  }

  closeRolePopover() {
    this.rolePopoverForId = null;
  }

  confirmPromote(user: UserModel) {
    this.closeRolePopover();
    const ok = confirm('Make this user a Representative?');
    if (ok) {
      this.promoteToRep(user);
    }
  }

  demoteToUser(user: UserModel) {
    if (!user || user.role === 'User') return;
    this.userService
      .updateUser({ id: Number(user.id), name: user.name, email: user.email, role: 'User' })
      .subscribe({
        next: (updated) => {
          this.users = this.users.map((u) => (u.id === updated.id ? updated : u));
          this.applyFilters();
          this.toast.success('Representative switched to User');
          this.closeRolePopover();
        },
        error: (err) => {
          console.error('Failed to switch role to User:', err);
          this.toast.error(err.error?.message || 'Failed to switch to User');
          this.closeRolePopover();
        },
      });
  }

  confirmDemote(user: UserModel) {
    this.closeRolePopover();
    const ok = confirm('Switch this Representative back to User?');
    if (ok) {
      this.demoteToUser(user);
    }
  }

  promoteToRep(user: UserModel) {
    if (!user || user.role === 'Rep') return;
    const confirmed = confirm(`Make "${user.name || user.email}" a Representative?`);
    if (!confirmed) return;

    this.userService
      .updateUser({ id: Number(user.id), name: user.name, email: user.email, role: 'Rep' })
      .subscribe({
        next: (updated) => {
          // Update local lists
          this.users = this.users.map((u) => (u.id === updated.id ? updated : u));
          this.applyFilters();
          this.toast.success('User promoted to Rep');
          this.closeRolePopover();
        },
        error: (err) => {
          console.error('Failed to promote user:', err);
          this.toast.error(err.error?.message || 'Failed to promote user');
          this.closeRolePopover();
        },
      });
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
