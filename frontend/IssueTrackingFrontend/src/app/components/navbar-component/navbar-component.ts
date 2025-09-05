import { Component, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { UserModel } from '../../models/user.model';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar-component.html',
  styleUrls: ['./navbar-component.css'],
})
export class NavbarComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  // Wait until user is hydrated, default null
  user = toSignal<UserModel | null>(this.auth.user$, { initialValue: null });

  // Dropdown state
  isDropdownOpen = false;

  get role() {
    return this.user()?.role ?? null;
  }

  isUser() {
    return this.role === 'User';
  }

  isRep() {
    return this.role === 'Representative';
  }

  isAdmin() {
    return this.role === 'Admin';
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  logout() {
    this.auth.logout();
    this.isDropdownOpen = false;
    this.router.navigate(['/login']);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    const target = event.target as HTMLElement;
    const dropdown = target.closest('.relative');
    if (!dropdown) {
      this.isDropdownOpen = false;
    }
  }
}
