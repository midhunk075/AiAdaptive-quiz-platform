import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);

  baseUrl = 'https://localhost:7069/api/';
  currentUser = signal<User | null>(null);

  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  removeCurrentUser() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }

  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map((user) => {
        if (user) this.setCurrentUser(user);
        return user;
      })
    );
  }

  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map((user) => {
        if (user) this.setCurrentUser(user);
        return user;
      })
    );
  }

  logout() {
    this.removeCurrentUser();
  }
}
