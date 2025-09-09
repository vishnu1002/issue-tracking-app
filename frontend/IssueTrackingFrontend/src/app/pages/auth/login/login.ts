import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.html',
})
export class Login implements OnInit {
  credentials = { email: '', password: '' };
  loading = false;
  error: string | null = null;
  redirectUrl: string | null = null;

  constructor(
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private toast: ToastService,
    private title: Title
  ) {}

  ngOnInit() {
    this.title.setTitle('Issue Tracker - Login');
    this.redirectUrl = this.route.snapshot.queryParamMap.get('redirect');
  }

  isFormValid(): boolean {
    return (
      this.credentials.email.trim() !== '' &&
      this.credentials.password.trim() !== '' &&
      this.credentials.password.length >= 6 &&
      this.isValidEmail(this.credentials.email)
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

    this.auth.login(this.credentials).subscribe({
      next: (res) => {
        this.loading = false;
        // Get role from AuthService after login
        const role = this.auth.getRole();
        this.toast.success('Signed in successfully');
        setTimeout(() => {
          const target = this.redirectUrl ?? '/dashboard';
          this.router.navigate([target]);
        }, 0);
      },
      error: (err) => {
        this.loading = false;
        this.error =
          err.error?.message || 'Login failed. Please check your credentials and try again.';
        this.toast.error(
          this.error || 'Login failed. Please check your credentials and try again.'
        );
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
