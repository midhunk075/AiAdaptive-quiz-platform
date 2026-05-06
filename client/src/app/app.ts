import { Component, inject, OnInit } from '@angular/core';
import { Nav } from './layout/nav/nav';
import { RouterOutlet } from '@angular/router';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [Nav, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private accountService = inject(AccountService);

  ngOnInit(): void {
    this.refreshUser();
  }

  refreshUser() {
    const userString = localStorage.getItem('user');
    if (userString) {
      const user = JSON.parse(userString);
      this.accountService.setCurrentUser(user);
    } else {
      this.accountService.removeCurrentUser();
    }
  }
}
