import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';
import { ApplicationUser } from '@core/models';

export type UserUpdateRequest = ApplicationUser;

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly usersUrl = `${environment.apiUrl}/Users`;

  getUserById(id: string): Observable<ApplicationUser> {
    return this.http.get<ApplicationUser>(`${this.usersUrl}/${id}`);
  }

  updateUser(id: string, userData: UserUpdateRequest): Observable<ApplicationUser> {
    return this.http.put<ApplicationUser>(`${this.usersUrl}/${id}`, userData);
  }
}
