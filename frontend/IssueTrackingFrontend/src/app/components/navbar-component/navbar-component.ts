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

  get initials(): string {
    const name = this.user()?.name ?? '';
    return this.computeInitials(name);
  }

  private computeInitials(name: string): string {
    const trimmed = name.trim();
    if (!trimmed) return 'U';
    const parts = trimmed.split(/\s+/).filter(Boolean);
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  }

  isUser() {
    return this.role === 'User';
  }

  isRep() {
    return this.role === 'Rep';
  }

  isAdmin() {
    return this.role === 'Admin';
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  closeDropdown() {
    this.isDropdownOpen = false;
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
