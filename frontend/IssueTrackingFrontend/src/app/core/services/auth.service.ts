import { Injectable } from '@angular/core';
import { BehaviorSubject, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { UserModel, Role } from '../../models/user.model';

interface JwtPayload {
  sub: string;
  email: string;
  name?: string;
  role: Role;
  exp?: number;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly tokenKey = 'auth_token';
  private readonly _user$ = new BehaviorSubject<UserModel | null>(null);
  readonly user$ = this._user$.asObservable();

  constructor(private http: HttpClient) {
    const token = this.getToken();
    if (token) this.hydrateFromToken(token);
  }

  login(credentials: { email: string; password: string }) {
    return this.http
      .post<{ token: string }>('/api/auth/login', credentials)
      .pipe(tap(({ token }) => this.setToken(token)));
  }

  register(data: { name: string; email: string; password: string }) {
    return this.http.post('/api/auth/register', data);
  }

  setToken(token: string) {
    localStorage.setItem(this.tokenKey, token);
    this.hydrateFromToken(token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
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
    if (!p) return;
    const user: UserModel = {
      id: p.sub,
      name: p.name ?? '',
      email: p.email,
      role: p.role,
    };
    this._user$.next(user);
  }

  private decodePayload(token: string): JwtPayload | null {
    try {
      const payload = token.split('.')[1] ?? '';
      const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(json) as JwtPayload;
    } catch {
      return null;
    }
  }
}
