import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserModel } from '../../models/user.model';
import { env } from '../../../env/env';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly baseUrl = env.apiUrl + '/user';

  constructor(private http: HttpClient) {}

  getAllUsers(): Observable<UserModel[]> {
    return this.http.get<UserModel[]>(this.baseUrl);
  }

  getRepresentatives(): Observable<UserModel[]> {
    return this.http.get<UserModel[]>(`${this.baseUrl}/representatives`);
  }

  getUser(id: string): Observable<UserModel> {
    return this.http.get<UserModel>(`${this.baseUrl}/${id}`);
  }

  updateUser(data: {
    id: number;
    name: string;
    email: string;
    role: string;
  }): Observable<UserModel> {
    return this.http.put<UserModel>(`${this.baseUrl}/${data.id}`, data);
  }

  updatePassword(id: number, currentPassword: string, newPassword: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}/password`, {
      currentPassword,
      newPassword,
    });
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
