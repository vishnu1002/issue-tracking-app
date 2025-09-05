import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const allowed: string[] = route.data?.['roles'] ?? [];
  const role = auth.getRole();

  if (role && allowed.includes(role)) return true;

  // Optional: redirect to a role-specific home; here we go to landing
  return router.createUrlTree(['/']);
};
