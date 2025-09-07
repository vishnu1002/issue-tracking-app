import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../core/services/user.service';
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

  users: UserModel[] = [];
  filteredUsers: UserModel[] = [];
  loading = false;
  error: string | null = null;

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
        this.applyFilters();
        this.loading = false;
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
        },
        error: (err) => {
          console.error('Failed to delete user:', err);

          // Handle specific error messages from the backend
          if (err.status === 400 && err.error?.message) {
            alert(err.error.message);
          } else if (err.status === 404) {
            alert('User not found.');
          } else {
            alert('Failed to delete user. Please try again.');
          }
        },
      });
    }
  }
}
