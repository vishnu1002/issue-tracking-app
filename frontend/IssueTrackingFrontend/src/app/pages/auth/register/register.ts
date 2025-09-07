import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  userData = { name: '', email: '', password: '' };
  confirmPassword = '';
  loading = false;
  error: string | null = null;
  success: string | null = null;

  constructor(private auth: AuthService, private router: Router) {}

  isFormValid(): boolean {
    return (
      this.userData.name.trim() !== '' &&
      this.userData.email.trim() !== '' &&
      this.userData.password.trim() !== '' &&
      this.confirmPassword.trim() !== '' &&
      this.userData.password.length >= 6 &&
      this.userData.password === this.confirmPassword &&
      this.isValidEmail(this.userData.email)
    );
  }

  isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  submit() {
    if (!this.isFormValid()) {
      return;
    }

    this.loading = true;
    this.error = null;
    this.success = null;

    this.auth.register(this.userData).subscribe({
      next: (res) => {
        this.loading = false;
        this.success = 'Account created successfully! Redirecting to login...';
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Registration failed. Please try again.';
      },
    });
  }

  togglePasswordVisibility(field: string) {
    const input = document.querySelector(`input[name="${field}"]`) as HTMLInputElement;
    if (input) {
      input.type = input.type === 'password' ? 'text' : 'password';
    }
  }
}
