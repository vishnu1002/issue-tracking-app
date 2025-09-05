import { Routes } from '@angular/router';

import { Login } from './pages/auth/login/login';
import { Register } from './pages/auth/register/register';

import { Landing } from './pages/landing/landing';
import { Tickets } from './pages/tickets/tickets';
import { History } from './pages/history/history';
import { Profile } from './pages/profile/profile';
import { Users } from './pages/users/users';

import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', component: Landing },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  {
    path: 'tickets',
    component: Tickets,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['User', 'Representative', 'Admin'] },
  },
  {
    path: 'history',
    component: History,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['User', 'Representative'] },
  },
  {
    path: 'users',
    component: Users,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
  },
  { path: 'profile', component: Profile },
  { path: '**', redirectTo: 'dashboard' },
];
