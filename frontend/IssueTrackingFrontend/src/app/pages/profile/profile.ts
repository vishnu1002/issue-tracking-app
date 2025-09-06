import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  auth = inject(AuthService);
  userService = inject(UserService);

  user = toSignal(this.auth.user$, { initialValue: null });

  profileData = {
    name: '',
    email: '',
    role: '',
  };

  passwordData = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  };

  updating = false;
  error: string | null = null;
  success: string | null = null;

  ngOnInit() {
    this.loadUserData();
  }

  loadUserData() {
    const currentUser = this.auth.getCurrentUser();
    if (currentUser) {
      this.profileData = {
        name: currentUser.name,
        email: currentUser.email,
        role: currentUser.role,
      };
    }
  }

  isFormValid(): boolean {
    return (
      this.profileData.name.trim() !== '' &&
      this.profileData.email.trim() !== '' &&
      this.isValidEmail(this.profileData.email)
    );
  }

  isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  updateProfile() {
    if (!this.isFormValid()) {
      this.error = 'Please fill in all required fields correctly';
      return;
    }

    this.updating = true;
    this.error = null;
    this.success = null;

    const currentUser = this.auth.getCurrentUser();
    if (!currentUser) {
      this.error = 'User not found';
      this.updating = false;
      return;
    }

    const updateData = {
      id: parseInt(currentUser.id),
      name: this.profileData.name,
      email: this.profileData.email,
      role: this.profileData.role,
    };

    this.userService.updateUser(updateData).subscribe({
      next: (updatedUser) => {
        this.updating = false;
        this.success = 'Profile updated successfully!';

        // Update the auth service with new user data
        this.auth.setToken(this.auth.getToken() || '');

        setTimeout(() => {
          this.success = null;
        }, 3000);
      },
      error: (err) => {
        this.updating = false;
        this.error = err.error?.message || 'Failed to update profile. Please try again.';
      },
    });
  }

  updatePassword() {
    if (
      !this.passwordData.currentPassword ||
      !this.passwordData.newPassword ||
      !this.passwordData.confirmPassword
    ) {
      this.error = 'Please fill in all password fields';
      return;
    }

    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.error = 'New passwords do not match';
      return;
    }

    if (this.passwordData.newPassword.length < 6) {
      this.error = 'New password must be at least 6 characters long';
      return;
    }

    this.updating = true;
    this.error = null;
    this.success = null;

    const currentUser = this.auth.getCurrentUser();
    if (!currentUser) {
      this.error = 'User not found';
      this.updating = false;
      return;
    }

    this.userService
      .updatePassword(
        parseInt(currentUser.id),
        this.passwordData.currentPassword,
        this.passwordData.newPassword
      )
      .subscribe({
        next: () => {
          this.updating = false;
          this.success = 'Password updated successfully!';
          this.passwordData = {
            currentPassword: '',
            newPassword: '',
            confirmPassword: '',
          };
          setTimeout(() => {
            this.success = null;
          }, 3000);
        },
        error: (err) => {
          this.updating = false;
          this.error =
            err.error?.message || 'Failed to update password. Please check your current password.';
        },
      });
  }

  resetForm() {
    this.loadUserData();
    this.passwordData = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    };
    this.error = null;
    this.success = null;
  }

  togglePasswordVisibility(field: string) {
    const input = document.querySelector(`input[name="${field}"]`) as HTMLInputElement;
    if (input) {
      input.type = input.type === 'password' ? 'text' : 'password';
    }
  }
}
