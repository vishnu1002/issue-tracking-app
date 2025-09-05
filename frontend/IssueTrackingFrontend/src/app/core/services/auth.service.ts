import { Injectable } from '@angular/core';
import { BehaviorSubject, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { UserModel, Role } from '../../models/user.model';
import { env } from '../../../env/env';

interface JwtPayload {
  // Standard JWT claims
  sub?: string;
  name?: string;
  email?: string;
  role?: Role;
  exp?: number;

  // .NET ClaimTypes (using full URIs)
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string;

  // Allow any other properties
  [key: string]: any;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly tokenKey = 'auth_token';
  private readonly _user$ = new BehaviorSubject<UserModel | null>(null);
  readonly user$ = this._user$.asObservable();

  private readonly baseUrl = env.apiUrl + '/auth';

  constructor(private http: HttpClient) {
    const token = this.getToken();
    if (token) this.hydrateFromToken(token);
  }

  login(credentials: { email: string; password: string }) {
    return this.http.post<{ token: string }>(`${this.baseUrl}/login`, credentials).pipe(
      tap(({ token }) => {
        console.log('Login successful, received token:', token);
        this.setToken(token);
      })
    );
  }

  register(data: { name: string; email: string; password: string }) {
    return this.http.post(`${this.baseUrl}/register`, data);
  }

  setToken(token: string) {
    console.log('Setting token:', token);
    localStorage.setItem(this.tokenKey, token);
    this.hydrateFromToken(token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    // Clear all localStorage items related to auth
    localStorage.clear();
    this._user$.next(null);
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    const payload = this.decodePayload(token);
    if (!payload) return false;
    if (payload.exp && payload.exp * 1000 <= Date.now()) return false;
    return true;
  }

  getRole(): Role | null {
    return this._user$.value?.role ?? null;
  }

  getCurrentUser(): UserModel | null {
    return this._user$.value;
  }

  private hydrateFromToken(token: string) {
    const p = this.decodePayload(token);
    if (!p) {
      console.log('No payload found in token');
      return;
    }
    console.log('Decoded payload:', p);

    // Map .NET claim types to user properties
    const user: UserModel = {
      id: p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || p.sub || '',
      name: p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || p.name || '',
      email:
        p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || p.email || '',
      role: (p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || p.role) as Role,
    };
    console.log('Created user object:', user);
    this._user$.next(user);
  }

  private decodePayload(token: string): JwtPayload | null {
    try {
      const payload = token.split('.')[1] ?? '';
      const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      const decoded = JSON.parse(json);
      console.log('JWT Payload:', decoded);
      return decoded as JwtPayload;
    } catch (error) {
      console.error('JWT decode error:', error);
      return null;
    }
  }
}
