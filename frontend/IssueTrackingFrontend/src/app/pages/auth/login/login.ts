import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // <-- for ngModel, ngForm
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule], // <-- include here
  templateUrl: './login.html',
})
export class Login {
  credentials = { email: '', password: '' };
  loading = false;
  error: string | null = null;
  redirectUrl: string | null = null;

  constructor(private auth: AuthService, private router: Router, private route: ActivatedRoute) {}

  ngOnInit() {
    this.redirectUrl = this.route.snapshot.queryParamMap.get('redirect');
  }

  submit() {
    this.loading = true;
    this.error = null;

    this.auth.login(this.credentials).subscribe({
      next: (res) => {
        this.loading = false;
        // Get role from AuthService after login
        const role = this.auth.getRole();
        setTimeout(() => {
          const target = this.redirectUrl ?? '/dashboard';
          this.router.navigate([target]);
        }, 0);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Login failed. Please try again.';
      },
    });
  }
}
