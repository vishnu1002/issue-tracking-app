import { Routes } from '@angular/router';
import { Login } from './pages/auth/login/login';
import { Register } from './pages/auth/register/register';

import { Dashboard } from './pages/dashboard/dashboard';

import { Landing } from './pages/landing/landing';
import { Tickets } from './pages/tickets/tickets';
import { Profile } from './pages/profile/profile';
import { Users } from './pages/users/users';

import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', component: Landing, pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },

  // Dashboard with nested routes
  {
    path: 'dashboard',
    component: Dashboard,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'tickets', pathMatch: 'full' },
      { path: 'tickets', component: Tickets },
      { path: 'profile', component: Profile },
      {
        path: 'users',
        component: Users,
        canActivate: [roleGuard],
        data: { roles: ['Admin'] },
      },
    ],
  },

  { path: '**', redirectTo: '' },
];